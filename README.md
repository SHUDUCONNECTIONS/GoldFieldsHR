# Digital Mining HR & Safety Management System

Enterprise HR & safety management system for mining sites — role-based web portal (Employee, Line Manager, HR, Safety Officer, Medical, Security, Executive) with a future companion mobile app.

Reference design: [docs/mockup.jpeg](docs/mockup.jpeg)

## Stack

- **Backend**: ASP.NET Core (.NET 9) Web API, EF Core, PostgreSQL
- **Frontend**: React + TypeScript (Vite)
- **Auth**: ASP.NET Core Identity + JWT

## Project structure

```
server/   ASP.NET Core solution (Api / Application / Domain / Infrastructure)
client/   React + TypeScript web portal
docs/     Reference materials
```

## Local development setup

### Prerequisites

- .NET 9 SDK
- Node.js 20+
- PostgreSQL 16+ (running locally, database `goldfields_hr_dev`)

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

### Backend tests

```
cd server
dotnet test tests/GoldFieldsHR.Infrastructure.Tests
```

Unit tests exercise the Application/Infrastructure service layer against an EF Core in-memory database, covering the workflow state machines and validation rules (Policies, Incidents, Performance, PPE, Permits, Emergency, Medical, Certificates).

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

Vitest + React Testing Library, covering pure utilities (`lib/format.ts`, `lib/csv.ts`) and interactive components (`Badge`, `ConfirmDialog`, `ToastProvider`). This is a foundational suite, not exhaustive page-level coverage — most page-level correctness in this project has instead been verified via real-browser Playwright sessions during development (see conversation history), not committed as an automated suite.

### Docker (local only)

```
docker compose up --build
```

Starts Postgres, the API (`http://localhost:5167`), and the web portal behind nginx (`http://localhost:5173`) as three containers. The API auto-applies EF Core migrations and seeds roles/site/bootstrap accounts on startup (same seeded accounts as above), so a fresh `docker compose up` needs no manual migration step.

This compose file is for local use only, not a production deployment: it runs the API in the `Development` environment (Swagger enabled, verbose EF logging), uses a hardcoded `Jwt__Key` and Postgres password defined directly in `docker-compose.yml`, and serves everything over plain HTTP. Replace those before deploying anywhere real.

## Modules

All sidebar modules are implemented end-to-end (backend + frontend): Dashboard, Timesheet, Work Shift, Leave Management, Safety & FLRA, Incidents & Near Miss, Policies & Documents, Medical, Training & Certifications, PPE Management, Permits to Work, Performance (KPI), My Certificates, Reports & Analytics, Emergency (SOS), Settings.

Known simplifications:
- Work Shift / Leave approvals surface a Line Manager's direct reports first (via `Employee.ManagerId`, set at registration or reassigned by HR in the Settings > Employee directory), but any Line Manager can still see and approve any pending request site-wide as a fallback — there is no hard restriction to direct reports only.
- Incident status transitions must move strictly forward (no backward or repeat transitions) but do not require passing through every intermediate stage.
