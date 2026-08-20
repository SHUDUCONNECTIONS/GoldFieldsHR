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
          `flex items-center gap-3 rounded-md px-3 py-2.5 text-sm font-medium shadow-sm transition-all ${
            isActive
              ? "bg-gradient-to-br from-yellow-300 via-amber-400 to-yellow-600 text-white shadow-md shadow-black/30 ring-1 ring-yellow-200/60"
              : "bg-gradient-to-br from-[#caa43d] to-[#8a6a1a] text-slate-900 hover:from-[#e0bb52] hover:to-[#a17d22] hover:text-slate-950"
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

  // Items with no category (e.g. Dashboard, Boards) render as permanent top-level
  // links rather than being tucked inside a collapsible group — a one-item
  // category would otherwise hide its only link behind an extra click.
  const topLevelItems = useMemo(
    () => NAV_ITEMS.filter((item) => !item.category && isVisibleToRole(item, session?.role)),
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

  function toggleGroup(label: string) {
    setOpenGroup((prev) => (prev === label ? null : label));
  }

  return (
    <aside
      className={`circuit-texture fixed inset-y-0 left-0 z-50 flex h-full w-72 shrink-0 flex-col overflow-hidden bg-slate-950 text-slate-300 transition-transform duration-200 ease-in-out lg:static lg:z-auto lg:w-64 lg:translate-x-0 ${
        isOpen ? "translate-x-0" : "-translate-x-full"
      }`}
    >
      <div className="relative flex flex-col items-center gap-2 border-b border-slate-800 px-4 py-6">
        <button
          type="button"
          onClick={onNavigate}
          aria-label="Close menu"
          className="absolute right-3 top-3 flex h-9 w-9 shrink-0 items-center justify-center rounded-md text-slate-400 hover:bg-slate-900 hover:text-slate-100 lg:hidden"
        >
          <X className="h-5 w-5" />
        </button>
        <img
          src={ramsLogo}
          alt="Rams Mining Technologies"
          className="h-16 w-auto drop-shadow-[0_0_16px_rgba(234,179,8,0.45)]"
        />
        <p className="text-center text-[11px] font-medium uppercase tracking-wide text-yellow-400/80">
          Engineering the Future of Mining.
        </p>
      </div>

      <nav className="relative flex-1 overflow-hidden px-2 py-3">
        <ul className="flex flex-col gap-0.5">
          {topLevelItems.map((item) => (
            <NavItemLink key={item.path} {...item} onNavigate={onNavigate} />
          ))}
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
