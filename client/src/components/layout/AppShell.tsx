import { Link, Outlet, useLocation } from "react-router-dom";
import { Siren } from "lucide-react";
import { Sidebar } from "./Sidebar";
import { NotificationBell } from "../NotificationBell";
import { HeaderSearch } from "../HeaderSearch";
import { MailDropdown } from "../MailDropdown";
import { ProfileDropdown } from "../ProfileDropdown";
import { NAV_ITEMS } from "../../config/nav";
import { useAuth } from "../../auth/AuthContext";
import { EmployeeRoleLabels } from "../../types/auth";

export function AppShell() {
  const location = useLocation();
  const { session } = useAuth();
  const currentLabel =
    NAV_ITEMS.find((item) => item.path === location.pathname)?.label ?? "Rams Mining Technologies";

  return (
    <div className="flex h-screen bg-slate-100">
      <Sidebar />
      <div className="flex flex-1 flex-col overflow-hidden">
        <header className="flex items-center justify-between gap-4 border-b border-slate-200 bg-white px-6 py-3">
          <div className="flex items-center gap-4">
            <h1 className="text-lg font-semibold text-slate-900">{currentLabel}</h1>
            <HeaderSearch />
          </div>
          <div className="flex items-center gap-3">
            {session && (
              <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-medium text-slate-600">
                {EmployeeRoleLabels[session.role]}
              </span>
            )}
            <MailDropdown />
            <NotificationBell />
            <Link
              to="/emergency"
              className="sos-pulse flex items-center gap-1.5 rounded-md bg-red-600 px-3 py-1.5 text-xs font-semibold text-white shadow-sm transition-colors hover:bg-red-500"
            >
              <Siren className="h-3.5 w-3.5" />
              SOS
            </Link>
            <ProfileDropdown />
          </div>
        </header>
        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
