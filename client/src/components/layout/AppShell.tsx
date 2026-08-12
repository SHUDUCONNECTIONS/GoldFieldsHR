import { useState } from "react";
import { Link, Outlet, useLocation } from "react-router-dom";
import { Menu, Siren } from "lucide-react";
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
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const currentLabel =
    NAV_ITEMS.find((item) => item.path === location.pathname)?.label ?? "Rams Mining Technologies";

  return (
    <div className="bg-cream flex h-screen">
      {isSidebarOpen && (
        <div
          className="fixed inset-0 z-40 bg-slate-950/60 lg:hidden"
          onClick={() => setIsSidebarOpen(false)}
          aria-hidden="true"
        />
      )}
      <Sidebar isOpen={isSidebarOpen} onNavigate={() => setIsSidebarOpen(false)} />
      <div className="flex flex-1 flex-col overflow-hidden">
        <header className="relative flex items-center justify-between gap-2 border-b border-slate-200 bg-gradient-to-r from-white via-white to-red-50/40 px-3 py-3 shadow-[0_1px_0_0_rgba(225,29,72,0.12)] sm:gap-4 sm:px-6">
          <div className="flex min-w-0 items-center gap-2 sm:gap-4">
            <button
              type="button"
              onClick={() => setIsSidebarOpen(true)}
              aria-label="Open menu"
              className="flex h-10 w-10 shrink-0 items-center justify-center rounded-md text-slate-500 hover:bg-slate-100 hover:text-slate-700 lg:hidden"
            >
              <Menu className="h-5 w-5" />
            </button>
            <h1 className="truncate text-base font-semibold text-slate-900 sm:text-lg">{currentLabel}</h1>
            <HeaderSearch />
          </div>
          <div className="flex shrink-0 items-center gap-1.5 sm:gap-3">
            {session && (
              <span className="hidden rounded-full bg-slate-100 px-3 py-1 text-xs font-medium text-slate-600 sm:inline-flex">
                {EmployeeRoleLabels[session.role]}
              </span>
            )}
            <MailDropdown />
            <NotificationBell />
            <Link
              to="/emergency"
              aria-label="Emergency SOS"
              className="sos-pulse flex h-10 w-10 items-center justify-center gap-1.5 rounded-md bg-red-600 text-xs font-semibold text-white shadow-sm transition-colors hover:bg-red-500 sm:h-auto sm:w-auto sm:px-3 sm:py-1.5"
            >
              <Siren className="h-4 w-4 sm:h-3.5 sm:w-3.5" />
              <span className="hidden sm:inline">SOS</span>
            </Link>
            <ProfileDropdown />
          </div>
        </header>
        <main className="flex-1 overflow-y-auto p-4 sm:p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
