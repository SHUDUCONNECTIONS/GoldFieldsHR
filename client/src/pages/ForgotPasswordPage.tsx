import { useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { Pickaxe } from "lucide-react";
import { forgotPassword } from "../api/auth";
import { extractErrorMessage } from "../api/client";

export function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState<string | null>(null);
  const [devResetToken, setDevResetToken] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setMessage(null);
    setDevResetToken(null);
    setIsSubmitting(true);
    try {
      const result = await forgotPassword(email);
      setMessage(result.message);
      if (result.devResetToken) {
        setDevResetToken(result.devResetToken);
      }
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-950 px-4">
      <div className="w-full max-w-sm rounded-lg bg-white p-8 shadow-xl">
        <div className="mb-6 flex items-center gap-2">
          <Pickaxe className="h-6 w-6 text-amber-500" />
          <div className="leading-tight">
            <p className="font-semibold text-slate-900">GoldFields HR</p>
            <p className="text-xs text-slate-500">Workforce. Safety. Performance.</p>
          </div>
        </div>

        <h2 className="mb-1 text-lg font-semibold text-slate-900">Forgot password</h2>
        <p className="mb-4 text-sm text-slate-500">
          Enter your account email and we'll generate a reset link.
        </p>

        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Email
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            />
          </label>

          {error && <p className="text-sm text-red-600">{error}</p>}
          {message && <p className="text-sm text-emerald-600">{message}</p>}

          {devResetToken && (
            <div className="rounded-md border border-amber-200 bg-amber-50 p-3 text-xs text-amber-800">
              <p className="mb-2 font-medium">
                No email provider is configured yet, so here's your reset link (development only):
              </p>
              <Link
                to={`/reset-password?email=${encodeURIComponent(email)}&token=${encodeURIComponent(devResetToken)}`}
                className="font-medium underline"
              >
                Continue to reset password
              </Link>
            </div>
          )}

          <button
            type="submit"
            disabled={isSubmitting}
            className="mt-2 rounded-md bg-slate-950 px-3 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-50"
          >
            {isSubmitting ? "Sending..." : "Send reset link"}
          </button>
        </form>

        <p className="mt-4 text-center text-sm text-slate-500">
          <Link to="/login" className="font-medium text-amber-600 hover:underline">
            Back to sign in
          </Link>
        </p>
      </div>
    </div>
  );
}
