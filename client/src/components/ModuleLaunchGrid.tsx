import { Link } from "react-router-dom";
import { NAV_ITEMS } from "../config/nav";

export function ModuleLaunchGrid() {
  const items = NAV_ITEMS.filter((item) => item.path !== "/" && item.path !== "/settings");

  return (
    <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-200 px-6 py-4">
        <h3 className="text-sm font-semibold text-slate-900">Core modules</h3>
      </div>
      <div className="grid grid-cols-2 gap-3 p-6 sm:grid-cols-3 lg:grid-cols-5">
        {items.map(({ path, label, icon: Icon, badge }) => (
          <Link
            key={path}
            to={path}
            className="group flex flex-col items-center gap-2 rounded-lg border border-slate-200 p-4 text-center transition-colors hover:border-amber-300 hover:bg-amber-50"
          >
            <span className="relative flex h-10 w-10 items-center justify-center rounded-lg bg-slate-100 text-slate-700 transition-colors group-hover:bg-amber-100 group-hover:text-amber-700">
              <Icon className="h-5 w-5" />
              {badge && (
                <span className="absolute -right-1.5 -top-1.5 rounded-full bg-amber-400 px-1 text-[9px] font-semibold text-slate-950">
                  {badge}
                </span>
              )}
            </span>
            <span className="text-xs font-medium text-slate-700">{label}</span>
          </Link>
        ))}
      </div>
    </div>
  );
}
