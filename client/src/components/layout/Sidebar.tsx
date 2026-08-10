import { NavLink } from "react-router-dom";
import { LogOut, Pickaxe } from "lucide-react";
import { NAV_ITEMS } from "../../config/nav";
import { useAuth } from "../../auth/AuthContext";
import { EmployeeRoleLabels } from "../../types/auth";

export function Sidebar() {
  const { session, signOut } = useAuth();

  return (
    <aside className="flex h-full w-64 shrink-0 flex-col bg-slate-950 text-slate-300">
      <div className="flex items-center gap-2 border-b border-slate-800 px-4 py-4">
        <Pickaxe className="h-6 w-6 text-amber-400" />
        <div className="leading-tight">
          <p className="text-sm font-semibold text-white">GoldFields HR</p>
          <p className="text-xs text-amber-400">Workforce. Safety. Performance.</p>
        </div>
      </div>

      <nav className="flex-1 overflow-y-auto px-2 py-3">
        <ul className="flex flex-col gap-0.5">
          {NAV_ITEMS.map(({ path, label, icon: Icon, badge }) => (
            <li key={path}>
              <NavLink
                to={path}
                end={path === "/"}
                className={({ isActive }) =>
                  `flex items-center gap-3 rounded-md px-3 py-2 text-sm transition-colors ${
                    isActive
                      ? "bg-amber-400/10 text-amber-300"
                      : "text-slate-400 hover:bg-slate-900 hover:text-slate-100"
                  }`
                }
              >
                <Icon className="h-4 w-4 shrink-0" />
                <span className="flex-1">{label}</span>
                {badge && (
                  <span className="rounded bg-amber-400 px-1.5 py-0.5 text-[10px] font-semibold text-slate-950">
                    {badge}
                  </span>
                )}
              </NavLink>
            </li>
          ))}
        </ul>
      </nav>

      {session && (
        <div className="border-t border-slate-800 px-4 py-3">
          <p className="truncate text-sm font-medium text-white">{session.fullName}</p>
          <p className="truncate text-xs text-slate-400">{EmployeeRoleLabels[session.role]}</p>
          <button
            type="button"
            onClick={signOut}
            className="mt-3 flex w-full items-center gap-2 rounded-md px-2 py-1.5 text-xs text-slate-400 hover:bg-slate-900 hover:text-slate-100"
          >
            <LogOut className="h-3.5 w-3.5" />
            Sign out
          </button>
        </div>
      )}
    </aside>
  );
}
