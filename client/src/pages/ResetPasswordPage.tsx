import { useState, type FormEvent } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { resetPassword } from "../api/auth";
import { extractErrorMessage } from "../api/client";
import ramsLogo from "../assets/rams-logo.png";

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [email, setEmail] = useState(searchParams.get("email") ?? "");
  const [token, setToken] = useState(searchParams.get("token") ?? "");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);

    if (newPassword !== confirmPassword) {
      setError("New password and confirmation do not match.");
      return;
    }

    setIsSubmitting(true);
    try {
      await resetPassword(email, token, newPassword);
      setSuccess(true);
      setTimeout(() => navigate("/login", { replace: true }), 2000);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="bg-cream relative flex min-h-screen items-center justify-center overflow-hidden px-4">
      <div className="soft-blobs">
        <span className="blob-tr" />
        <span className="blob-bl" />
      </div>
      <div className="fade-in-up relative z-10 w-full max-w-sm overflow-hidden rounded-2xl bg-white p-8 shadow-xl">
        <span className="absolute inset-x-0 top-0 h-1.5 bg-gradient-to-r from-red-500 via-red-600 to-slate-900" />
        <div className="mb-6 flex items-center gap-2">
          <img src={ramsLogo} alt="Rams Mining Technologies" className="h-9 w-auto" />
          <p className="text-xs text-slate-500">Engineering the Future of Mining.</p>
        </div>

        <h2 className="mb-4 text-lg font-semibold text-slate-900">Reset password</h2>

        {success ? (
          <p className="text-sm text-emerald-600">Password reset. Redirecting to sign in...</p>
        ) : (
          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <label className="flex flex-col gap-1 text-sm text-slate-700">
              Email
              <input
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="rounded-md border border-slate-300 px-3 py-2 text-sm transition-shadow focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
              />
            </label>

            <label className="flex flex-col gap-1 text-sm text-slate-700">
              Reset token
              <textarea
                required
                rows={3}
                value={token}
                onChange={(e) => setToken(e.target.value)}
                className="rounded-md border border-slate-300 px-3 py-2 font-mono text-xs transition-shadow focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
              />
            </label>

            <label className="flex flex-col gap-1 text-sm text-slate-700">
              New password
              <input
                type="password"
                required
                minLength={8}
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                className="rounded-md border border-slate-300 px-3 py-2 text-sm transition-shadow focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
              />
            </label>

            <label className="flex flex-col gap-1 text-sm text-slate-700">
              Confirm new password
              <input
                type="password"
                required
                minLength={8}
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="rounded-md border border-slate-300 px-3 py-2 text-sm transition-shadow focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
              />
            </label>

            {error && <p className="text-sm text-red-600">{error}</p>}

            <button
              type="submit"
              disabled={isSubmitting}
              className="mt-2 rounded-md bg-red-600 px-3 py-2 text-sm font-medium text-white hover:bg-red-500 disabled:opacity-50"
            >
              {isSubmitting ? "Resetting..." : "Reset password"}
            </button>
          </form>
        )}

        <p className="mt-4 text-center text-sm text-slate-500">
          <Link to="/login" className="font-medium text-red-600 hover:underline">
            Back to sign in
          </Link>
        </p>
      </div>
    </div>
  );
}
