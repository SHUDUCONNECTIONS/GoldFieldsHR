import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { apiClient, REFRESH_TOKEN_STORAGE_KEY, SESSION_EXPIRED_EVENT, TOKEN_STORAGE_KEY } from "../api/client";
import { useToast } from "../components/ToastProvider";
import type { AuthResponse } from "../types/auth";

const SESSION_STORAGE_KEY = "goldfields.session";

interface Session {
  employeeId: string;
  fullName: string;
  role: AuthResponse["role"];
  expiresAtUtc: string;
}

interface AuthContextValue {
  session: Session | null;
  isAuthenticated: boolean;
  signIn: (auth: AuthResponse) => void;
  signOut: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function readStoredSession(): Session | null {
  const raw = localStorage.getItem(SESSION_STORAGE_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as Session;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(() => readStoredSession());
  const { showError } = useToast();

  const signIn = useCallback((auth: AuthResponse) => {
    const nextSession: Session = {
      employeeId: auth.employeeId,
      fullName: auth.fullName,
      role: auth.role,
      expiresAtUtc: auth.expiresAtUtc,
    };
    localStorage.setItem(TOKEN_STORAGE_KEY, auth.accessToken);
    localStorage.setItem(REFRESH_TOKEN_STORAGE_KEY, auth.refreshToken);
    localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(nextSession));
    setSession(nextSession);
  }, []);

  const signOut = useCallback(() => {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_STORAGE_KEY);
    if (refreshToken) {
      apiClient.post("/auth/logout", { refreshToken }).catch(() => {
        // Best-effort server-side revocation; local session is cleared regardless.
      });
    }
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    localStorage.removeItem(REFRESH_TOKEN_STORAGE_KEY);
    localStorage.removeItem(SESSION_STORAGE_KEY);
    setSession(null);
  }, []);

  useEffect(() => {
    function handleSessionExpired() {
      const wasSignedIn = session !== null;
      signOut();
      if (wasSignedIn) {
        showError("Your session has expired. Please sign in again.");
      }
    }
    window.addEventListener(SESSION_EXPIRED_EVENT, handleSessionExpired);
    return () => window.removeEventListener(SESSION_EXPIRED_EVENT, handleSessionExpired);
  }, [signOut, session, showError]);

  const value = useMemo<AuthContextValue>(
    () => ({ session, isAuthenticated: session !== null, signIn, signOut }),
    [session, signIn, signOut],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
