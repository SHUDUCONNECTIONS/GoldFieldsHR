# Digital Mining HR & Safety Management System

Enterprise HR & safety management system for mining sites — role-based web portal (Employee, Line Manager, HR, Safety Officer, Medical, Security, Executive) with a future companion mobile app.

Reference design: [docs/mockup.jpeg](docs/mockup.jpeg)

## Stack

- **Backend**: ASP.NET Core (.NET 9) Web API, EF Core, PostgreSQL
- **Frontend**: React + TypeScript (Vite)
- **Auth**: ASP.NET Core Identity + JWT

## Project structure

```
server/                        ASP.NET Core solution (Api / Application / Domain / Infrastructure)
server/ClockingReportParser/   Python service the Timesheet page's clocking-report parser calls
client/                        React + TypeScript web portal
docs/                          Reference materials
```

## Local development setup

### Prerequisites

- .NET 9 SDK
- Node.js 20+
- PostgreSQL 16+ (running locally, database `goldfields_hr_dev`)
- Python 3.12+ (only needed for the clocking-report parser — see below)

### Backend

```
cd server
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=goldfields_hr_dev;Username=goldfields_app;Password=<your-password>" --project src/GoldFieldsHR.Api
dotnet user-secrets set "Jwt:Key" "<a-long-random-signing-secret>" --project src/GoldFieldsHR.Api
dotnet ef database update --project src/GoldFieldsHR.Infrastructure --startup-project src/GoldFieldsHR.Api
dotnet run --project src/GoldFieldsHR.Api
```

`Jwt:Issuer`/`Jwt:Audience`/`Jwt:ExpiryMinutes` have defaults in `appsettings.json`; `Jwt:Key` and `ConnectionStrings:Default` are dev secrets and must be set via `dotnet user-secrets` (never committed).

On startup in Development, the API auto-seeds the `EmployeeRole` Identity roles, a default site ("Kusasalethu Mine"), and two bootstrap admin accounts so the app is usable from a fresh database:

| Email | Password | Role |
| --- | --- | --- |
| `hr.admin@goldfieldshr.local` | `Bootstrap@123` | HR |
| `exec.admin@goldfieldshr.local` | `Bootstrap@123` | Executive |

Dev-only credentials — change or remove `DatabaseSeeder.SeedBootstrapAccountAsync` before any non-local deployment. `POST /api/auth/register` (public self-registration) only ever creates `Employee`-role accounts; the HR bootstrap account (or any account HR promotes) can assign a different role to an existing employee via `PATCH /api/employees/{id}/role`.

API available at `http://localhost:5167` (Swagger UI at `/swagger` in Development).

Logging is structured via Serilog: requests and application events are written to the console and to rolling daily files under `server/src/GoldFieldsHR.Api/logs/` (gitignored, 14-day retention). Configure levels/sinks under the `Serilog` section in `appsettings.json`.

Production hardening baked in:
- **CORS** — only origins listed under `Cors:AllowedOrigins` in `appsettings.json` (default `http://localhost:5173`) may call the API from a browser.
- **Rate limiting** — all `/api/auth/*` endpoints are limited to 10 requests/minute, partitioned per client IP (one caller exhausting their limit doesn't affect anyone else); excess requests get `429 Too Many Requests`.
- **Account lockout** — 5 failed login attempts locks an account for 15 minutes (`UserManager` lockout tracking), independent of and in addition to the IP-based rate limit.
- **Global exception handling** — unhandled exceptions return a generic `ProblemDetails` 500 response (never a stack trace) and are logged via Serilog.
- **Health check** — `GET /healthz` reports API + database connectivity as JSON, unauthenticated, for load balancers/orchestrators.
- **HSTS** — enabled for all non-Development environments.
- **Security response headers** — `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy` on every API response (and on the nginx-served frontend in Docker).
- **Input validation** — FluentValidation covers every write request DTO across all modules; malformed input returns `400` with per-field messages before it reaches a service.
- **Refresh-token cleanup** — a background service sweeps expired refresh tokens out of the database every 24 hours.

### Backend tests

```
cd server
dotnet test
```

Runs both backend test projects:
- **`GoldFieldsHR.Infrastructure.Tests`** — unit tests for the Application/Infrastructure service layer against an EF Core in-memory database, covering workflow state machines and validation rules (Auth, Policies, Incidents, Performance, PPE, Legal Appointments, Emergency, Medical, Certificates, Sites, Notifications, Attachments, Timesheet, Employees).
- **`GoldFieldsHR.Api.Tests`** — integration tests hosting the real API pipeline via `WebApplicationFactory` (EF InMemory swapped in for Postgres), covering CORS, rate limiting, the FluentValidation action filter, and the global exception handler end-to-end through actual HTTP requests — regression coverage for the middleware pipeline itself, not just the service layer.

### Clocking report parser service

The Timesheet page's HR-only "Clocking report parser" (upload an Individual Clocking History PDF from the site's turnstile system, download a formatted Timesheet workbook) is a Python FastAPI service the API proxies to — not reimplemented in C#, since the PDF layout parsing and overtime/rotating-shift business rules are non-trivial and already working. `GoldFieldsHR.Api.Controllers.TimesheetController`'s `POST /api/timesheet/clocking-report-parser` forwards the upload to it over HTTP (`ClockingParser:BaseUrl`, default `http://localhost:8010`); it has no auth of its own, so it must never be reachable from outside the deployment.

```
cd server/ClockingReportParser
pip install -r requirements.txt
uvicorn main:app --port 8010
```

Runs alongside the .NET API and Postgres — start it before (or alongside) `dotnet run` if you want the parser panel to work locally. Everything else on the Timesheet page works without it.

### Frontend

```
cd client
npm install
npm run dev
```

Web portal available at `http://localhost:5173`.

### Frontend tests

```
cd client
npm test
```

Vitest + React Testing Library, covering pure utilities (`lib/format.ts`, `lib/csv.ts`) and interactive components (`Badge`, `ConfirmDialog`, `ToastProvider`). This is a foundational suite, not exhaustive page-level coverage.

### End-to-end tests (local only)

```
cd client
npx playwright install chromium   # first time only
npm run test:e2e
```

Playwright specs under `client/e2e/` cover login (valid/invalid credentials, logout), the dashboard, and Employee Directory search/pagination, run against a real running backend + frontend (start both first, per above). Not wired into CI — CI would need a live Postgres, migrated/seeded API, and frontend server orchestrated together, which is a meaningfully different (and unverified, in this environment) pipeline from the plain build/test jobs that run today.

### Docker (local only)

```
docker compose up --build
```

Starts Postgres, the clocking report parser, the API (`http://localhost:5167`), and the web portal behind nginx (`http://localhost:5173`) as four containers. The API auto-applies EF Core migrations and seeds roles/site/bootstrap accounts on startup (same seeded accounts as above), so a fresh `docker compose up` needs no manual migration step.

This compose file is for local use only, not a production deployment: it runs the API in the `Development` environment (Swagger enabled, verbose EF logging), uses a hardcoded `Jwt__Key` and Postgres password defined directly in `docker-compose.yml`, and serves everything over plain HTTP. Replace those before deploying anywhere real.

### Deploying to Render (shared testing URL)

`render.yaml` at the repo root is a [Render Blueprint](https://render.com/docs/blueprint-spec) that provisions the whole stack — Postgres, the clocking report parser, the API, and the frontend — from this repo in one go, so people other than you can reach the app over the internet instead of just `localhost`.

1. Push this repo to GitHub (Render deploys from a Git remote, not a local working copy).
2. In the [Render dashboard](https://dashboard.render.com), click **New +** -> **Blueprint** and point it at the repo. Render reads `render.yaml` and shows a preview of the 4 resources it's about to create (`goldfields-hr-db`, `goldfields-hr-clocking-parser`, `goldfields-hr-api`, `goldfields-hr-client`) — click **Apply**. `goldfields-hr-clocking-parser` is a private service (no public URL) — it has no auth of its own, so it must only ever be reachable from `goldfields-hr-api` over Render's internal network, never from the internet.
3. First deploy takes a few minutes (Postgres provisioning + three Docker/npm builds). Once the API service is live, open its **Logs** tab and confirm you see `Applying migration...` and no seeding errors — that means the bootstrap accounts below now exist on the live database.
4. Open the client service's URL (shown on its dashboard page, `https://goldfields-hr-client.onrender.com` unless the name was suffixed — see step 5) and sign in with the same bootstrap credentials as local dev:

   | Email | Password | Role |
   | --- | --- | --- |
   | `hr.admin@goldfieldshr.local` | `Bootstrap@123` | HR |
   | `exec.admin@goldfieldshr.local` | `Bootstrap@123` | Executive |

   **Change these passwords (Settings > Change password) before sharing the URL with real testers** — they're the same publicly-documented defaults used locally.
5. If Render suffixed either service's name (only happens if `goldfields-hr-api`/`-client` were already taken), the API's `Cors__AllowedOrigins__0` env var and the client's `VITE_API_BASE_URL` build-time env var (both set in `render.yaml`) won't match the real URLs. Update them in each service's **Environment** tab to the actual URLs shown on the dashboard, then trigger a manual redeploy of both (the client needs a rebuild since `VITE_API_BASE_URL` is baked into the JS bundle at build time, not read at runtime).

Free-tier caveats — fine for testing, revisit before treating this as a real deployment:
- The free Postgres database is deleted after Render's free trial period. Upgrade its plan, or swap `ConnectionStrings__Default` for an external always-free provider (e.g. Neon, Supabase), before relying on it beyond that window.
- Free web services spin down after 15 minutes idle; the next request wakes them back up with a ~30-60s cold start — expect that delay on the first login of the day.
- The API runs in `Production` there (unlike the Docker Compose setup above), so Swagger is disabled and HSTS is on — matches how a real deployment should look, just backed by free-tier infra.

**Troubleshooting a failed first deploy:** check the API service's Logs tab. A database connection failure on startup almost always means `ConnectionStrings__Default` (populated automatically from `fromDatabase` in `render.yaml`) doesn't match what Npgsql expects — Render provides it in `postgres://user:password@host/dbname` URI form, which Npgsql accepts natively, but if that specific string ever needs adjusting, the Postgres service's own Info tab shows the individual host/port/user/password/database fields you'd use to build an equivalent `Host=...;Port=...;Database=...;Username=...;Password=...` value by hand.

## Modules

All sidebar modules are implemented end-to-end (backend + frontend): Dashboard, Timesheet, Work Shift, Leave Management, Safety & FLRA, Incidents & Near Miss, Policies & Documents, Medical, Training & Certifications, PPE Management, Legal Appointments, Performance (KPI), My Certificates, Reports & Analytics, Emergency (SOS), Settings.

- **Timesheet** has no self-service clock-in/out — attendance comes from the site's turnstile system. HR uploads that system's Individual Clocking History PDF via the "Clocking report parser" panel and downloads a formatted Timesheet workbook (see "Clocking report parser service" above); parsed shifts aren't imported back into this app's own Timesheet History.
- **Work Shift**'s "Post a schedule" is a document upload (HR posts a title + attaches the roster as a PDF/image for everyone to view/download), not an in-app roster builder.

Known simplifications:
- Work Shift / Leave approvals surface a Line Manager's direct reports first (via `Employee.ManagerId`, set at registration or reassigned by HR in the Settings > Employee directory), but any Line Manager can still see and approve any pending request site-wide as a fallback — there is no hard restriction to direct reports only.
- Incident status transitions must move strictly forward (no backward or repeat transitions) but do not require passing through every intermediate stage.
