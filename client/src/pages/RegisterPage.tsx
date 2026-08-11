import { useEffect, useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { getSites, register } from "../api/auth";
import { extractErrorMessage } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { EmployeeRole, EmployeeRoleLabels, type Site } from "../types/auth";
import ramsLogo from "../assets/rams-logo.png";

const initialForm = {
  firstName: "",
  lastName: "",
  email: "",
  password: "",
  employeeNumber: "",
  jobTitle: "",
  role: EmployeeRole.Employee,
  siteId: "",
  managerEmployeeNumber: "",
  requestedRole: EmployeeRole.Employee as EmployeeRole,
};

export function RegisterPage() {
  const { signIn } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState(initialForm);
  const [sites, setSites] = useState<Site[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    getSites()
      .then((fetched) => {
        setSites(fetched);
        if (fetched.length > 0) {
          setForm((prev) => ({ ...prev, siteId: fetched[0].id }));
        }
      })
      .catch(() => setError("Could not load sites. Is the API running?"));
  }, []);

  function updateField<K extends keyof typeof initialForm>(key: K, value: (typeof initialForm)[K]) {
    setForm((prev) => ({ ...prev, [key]: value }));
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      const auth = await register({
        ...form,
        managerEmployeeNumber: form.managerEmployeeNumber || undefined,
        requestedRole: form.requestedRole === EmployeeRole.Employee ? undefined : form.requestedRole,
      });
      signIn(auth);
      navigate("/", { replace: true });
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="relative flex min-h-screen items-center justify-center overflow-hidden bg-slate-950 px-4 py-10">
      <div className="gradient-glow" />
      <div className="fade-in-up relative z-10 w-full max-w-md rounded-lg bg-white p-8 shadow-xl">
        <div className="mb-6 flex items-center gap-2">
          <img src={ramsLogo} alt="Rams Mining Technologies" className="h-9 w-auto" />
          <p className="text-xs text-slate-500">Engineering the Future of Mining.</p>
        </div>

        <h2 className="mb-4 text-lg font-semibold text-slate-900">Create account</h2>

        <form onSubmit={handleSubmit} className="flex flex-col gap-3">
          <div className="grid grid-cols-2 gap-3">
            <label className="flex flex-col gap-1 text-sm text-slate-700">
              First name
              <input
                required
                value={form.firstName}
                onChange={(e) => updateField("firstName", e.target.value)}
                className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm text-slate-700">
              Last name
              <input
                required
                value={form.lastName}
                onChange={(e) => updateField("lastName", e.target.value)}
                className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
              />
            </label>
          </div>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Email
            <input
              type="email"
              required
              value={form.email}
              onChange={(e) => updateField("email", e.target.value)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Password
            <input
              type="password"
              required
              minLength={8}
              value={form.password}
              onChange={(e) => updateField("password", e.target.value)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            />
          </label>

          <div className="grid grid-cols-2 gap-3">
            <label className="flex flex-col gap-1 text-sm text-slate-700">
              Employee #
              <input
                required
                value={form.employeeNumber}
                onChange={(e) => updateField("employeeNumber", e.target.value)}
                className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm text-slate-700">
              Job title
              <input
                required
                value={form.jobTitle}
                onChange={(e) => updateField("jobTitle", e.target.value)}
                className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
              />
            </label>
          </div>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Site
            <select
              required
              value={form.siteId}
              onChange={(e) => updateField("siteId", e.target.value)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            >
              {sites.map((site) => (
                <option key={site.id} value={site.id}>
                  {site.name}
                </option>
              ))}
            </select>
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Manager&apos;s employee # (optional)
            <input
              value={form.managerEmployeeNumber}
              onChange={(e) => updateField("managerEmployeeNumber", e.target.value)}
              placeholder="e.g. LM-1024"
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Desired role (optional)
            <select
              value={form.requestedRole}
              onChange={(e) => updateField("requestedRole", Number(e.target.value) as EmployeeRole)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            >
              {Object.entries(EmployeeRoleLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
          </label>

          <p className="text-xs text-slate-500">
            New accounts always start with the Employee role. If you select a different role above, it's recorded as
            a request for HR to review and grant &mdash; it isn't applied automatically.
          </p>

          {error && <p className="text-sm text-red-600">{error}</p>}

          <button
            type="submit"
            disabled={isSubmitting || !form.siteId}
            className="mt-2 rounded-md bg-slate-950 px-3 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-50"
          >
            {isSubmitting ? "Creating account..." : "Create account"}
          </button>
        </form>

        <p className="mt-4 text-center text-sm text-slate-500">
          Already have an account?{" "}
          <Link to="/login" className="font-medium text-amber-600 hover:underline">
            Sign in
          </Link>
        </p>
      </div>
    </div>
  );
}
