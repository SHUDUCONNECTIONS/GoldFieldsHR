import { useEffect, useMemo, useState } from "react";
import { NavLink, useLocation } from "react-router-dom";
import { ChevronDown, LogOut, X } from "lucide-react";
import { NAV_GROUPS, NAV_ITEMS, type NavItem } from "../../config/nav";
import { useAuth } from "../../auth/AuthContext";
import { EmployeeRoleLabels, type EmployeeRole } from "../../types/auth";
import ramsLogo from "../../assets/rams-logo-gold.png";

function isVisibleToRole(item: NavItem, role: EmployeeRole | undefined): boolean {
  return !item.roles || (role !== undefined && item.roles.includes(role));
}

interface SidebarProps {
  isOpen: boolean;
  onNavigate: () => void;
}

function NavItemLink({ path, label, icon: Icon, badge, onNavigate }: NavItem & { onNavigate: () => void }) {
  return (
    <li>
      <NavLink
        to={path}
        end={path === "/"}
        onClick={onNavigate}
        className={({ isActive }) =>
          `flex items-center gap-3 rounded-md px-3 py-2.5 text-sm font-medium transition-colors ${
            isActive
              ? "bg-yellow-500 text-white shadow-sm"
              : "text-slate-400 hover:bg-slate-900 hover:text-slate-100"
          }`
        }
      >
        <Icon className="h-4 w-4 shrink-0" />
        <span className="flex-1">{label}</span>
        {badge && (
          <span className="rounded bg-white px-1.5 py-0.5 text-[10px] font-semibold text-slate-900">
            {badge}
          </span>
        )}
      </NavLink>
    </li>
  );
}

export function Sidebar({ isOpen, onNavigate }: SidebarProps) {
  const { session, signOut } = useAuth();
  const location = useLocation();

  const visibleGroups = useMemo(
    () =>
      NAV_GROUPS.map((group) => ({
        ...group,
        items: group.items.filter((item) => isVisibleToRole(item, session?.role)),
      })).filter((group) => group.items.length > 0),
    [session?.role],
  );

  // Only one group open at a time (accordion), defaulting to whichever group
  // contains the current route. This keeps the sidebar's height bounded to
  // "header + item + 4 group labels + one group's items + footer" no matter
  // how many nav items exist, so it never needs to scroll.
  const [openGroup, setOpenGroup] = useState<string | null>(
    () => visibleGroups.find((group) => group.items.some((item) => item.path === location.pathname))?.label ?? null,
  );

  useEffect(() => {
    const activeGroup = visibleGroups.find((group) => group.items.some((item) => item.path === location.pathname));
    if (activeGroup) {
      setOpenGroup(activeGroup.label);
    }
  }, [location.pathname, visibleGroups]);

  const dashboardItem = NAV_ITEMS.find((item) => item.path === "/")!;

  function toggleGroup(label: string) {
    setOpenGroup((prev) => (prev === label ? null : label));
  }

  return (
    <aside
      className={`circuit-texture fixed inset-y-0 left-0 z-50 flex h-full w-72 shrink-0 flex-col overflow-hidden bg-slate-950 text-slate-300 transition-transform duration-200 ease-in-out lg:static lg:z-auto lg:w-64 lg:translate-x-0 ${
        isOpen ? "translate-x-0" : "-translate-x-full"
      }`}
    >
      <div className="relative flex items-center gap-2 border-b border-slate-800 px-4 py-4">
        <img src={ramsLogo} alt="Rams Mining Technologies" className="h-9 w-auto drop-shadow-[0_0_12px_rgba(234,179,8,0.35)]" />
        <p className="text-xs leading-tight text-yellow-400">Engineering the Future of Mining.</p>
        <button
          type="button"
          onClick={onNavigate}
          aria-label="Close menu"
          className="ml-auto flex h-9 w-9 shrink-0 items-center justify-center rounded-md text-slate-400 hover:bg-slate-900 hover:text-slate-100 lg:hidden"
        >
          <X className="h-5 w-5" />
        </button>
      </div>

      <nav className="relative flex-1 overflow-hidden px-2 py-3">
        <ul className="flex flex-col gap-0.5">
          <NavItemLink {...dashboardItem} onNavigate={onNavigate} />
        </ul>

        {visibleGroups.map((group) => {
          const isOpenGroup = openGroup === group.label;
          return (
            <div key={group.label} className="mt-3">
              <button
                type="button"
                onClick={() => toggleGroup(group.label)}
                className="flex w-full items-center justify-between rounded-md px-3 py-2 text-[11px] font-semibold uppercase tracking-wide text-slate-500 transition-colors hover:text-slate-300"
              >
                {group.label}
                <ChevronDown
                  className={`h-3.5 w-3.5 shrink-0 transition-transform duration-200 ${isOpenGroup ? "" : "-rotate-90"}`}
                />
              </button>
              {isOpenGroup && (
                <ul className="flex flex-col gap-0.5">
                  {group.items.map((item) => (
                    <NavItemLink key={item.path} {...item} onNavigate={onNavigate} />
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
            className="mt-3 flex w-full items-center gap-2 rounded-md px-2 py-2 text-xs text-slate-400 hover:bg-slate-900 hover:text-slate-100"
          >
            <LogOut className="h-3.5 w-3.5" />
            Sign out
          </button>
        </div>
      )}
    </aside>
  );
}
