import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { login } from "../api/auth";
import { extractErrorMessage } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import ramsLogo from "../assets/rams-logo.png";

export function LoginPage() {
  const { signIn } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      const auth = await login({ email, password });
      signIn(auth);
      navigate("/", { replace: true });
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
        <span className="absolute inset-x-0 top-0 h-1.5 bg-gradient-to-r from-yellow-500 via-yellow-600 to-slate-900" />
        <div className="mb-6 flex items-center gap-2">
          <img src={ramsLogo} alt="Rams Mining Technologies" className="h-9 w-auto" />
          <p className="text-xs text-slate-500">Engineering the Future of Mining.</p>
        </div>

        <h2 className="mb-4 text-lg font-semibold text-slate-900">Sign in</h2>

        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Email
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm transition-shadow focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Password
            <input
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm transition-shadow focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>

          <Link to="/forgot-password" className="self-end text-xs font-medium text-yellow-600 hover:underline">
            Forgot password?
          </Link>

          {error && <p className="text-sm text-red-600">{error}</p>}

          <button
            type="submit"
            disabled={isSubmitting}
            className="mt-2 rounded-md bg-yellow-600 px-3 py-2 text-sm font-medium text-white hover:bg-yellow-500 disabled:opacity-50"
          >
            {isSubmitting ? "Signing in..." : "Sign in"}
          </button>
        </form>

        <p className="mt-4 text-center text-sm text-slate-500">
          No account?{" "}
          <Link to="/register" className="font-medium text-yellow-600 hover:underline">
            Register
          </Link>
        </p>
      </div>
    </div>
  );
}
