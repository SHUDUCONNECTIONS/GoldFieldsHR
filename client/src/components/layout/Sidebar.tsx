import { useState } from "react";
import { NavLink } from "react-router-dom";
import { ChevronDown, LogOut } from "lucide-react";
import { NAV_GROUPS, NAV_ITEMS, type NavItem } from "../../config/nav";
import { useAuth } from "../../auth/AuthContext";
import { EmployeeRoleLabels } from "../../types/auth";
import ramsLogo from "../../assets/rams-logo.png";

function NavItemLink({ path, label, icon: Icon, badge }: NavItem) {
  return (
    <li>
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
  );
}

export function Sidebar() {
  const { session, signOut } = useAuth();
  const [collapsedGroups, setCollapsedGroups] = useState<Record<string, boolean>>({});

  const dashboardItem = NAV_ITEMS.find((item) => item.path === "/")!;

  function toggleGroup(label: string) {
    setCollapsedGroups((prev) => ({ ...prev, [label]: !prev[label] }));
  }

  return (
    <aside className="flex h-full w-64 shrink-0 flex-col bg-slate-950 text-slate-300">
      <div className="flex items-center gap-2 border-b border-slate-800 px-4 py-4">
        <div className="rounded-md bg-white px-2 py-1.5 shadow-[0_0_16px_-2px_rgba(245,158,11,0.5)]">
          <img src={ramsLogo} alt="Rams Mining Technologies" className="h-6 w-auto" />
        </div>
        <p className="text-xs leading-tight text-amber-400">Engineering the Future of Mining.</p>
      </div>

      <nav className="flex-1 overflow-y-auto px-2 py-3">
        <ul className="flex flex-col gap-0.5">
          <NavItemLink {...dashboardItem} />
        </ul>

        {NAV_GROUPS.map((group) => {
          const isCollapsed = collapsedGroups[group.label] ?? false;
          return (
            <div key={group.label} className="mt-3">
              <button
                type="button"
                onClick={() => toggleGroup(group.label)}
                className="flex w-full items-center justify-between rounded-md px-3 py-1.5 text-[11px] font-semibold uppercase tracking-wide text-slate-500 transition-colors hover:text-slate-300"
              >
                {group.label}
                <ChevronDown
                  className={`h-3.5 w-3.5 shrink-0 transition-transform duration-200 ${isCollapsed ? "-rotate-90" : ""}`}
                />
              </button>
              {!isCollapsed && (
                <ul className="flex flex-col gap-0.5">
                  {group.items.map((item) => (
                    <NavItemLink key={item.path} {...item} />
                  ))}
                </ul>
              )}
            </div>
          );
        })}
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
