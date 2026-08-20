import { NavLink, Outlet } from "react-router-dom";

const TABS = [
  { to: "/kpi", label: "Appraisals", end: true },
  { to: "/kpi/boards", label: "Boards", end: false },
  { to: "/kpi/performance", label: "Performance", end: true },
];

export function KpiHubPage() {
  return (
    <div className="stagger-children flex flex-col gap-4 rounded-2xl bg-[#202325] p-6 font-['Inter']">
      <div className="flex w-fit gap-1.5 rounded-lg border border-white/10 bg-[#3a3d40] p-1">
        {TABS.map((tab) => (
          <NavLink
            key={tab.to}
            to={tab.to}
            end={tab.end}
            className={({ isActive }) =>
              `rounded-md px-4 py-2 text-sm font-medium transition-colors ${
                isActive ? "bg-[#3f7429] text-[#eaf6de]" : "text-white/50 hover:bg-white/5"
              }`
            }
          >
            {tab.label}
          </NavLink>
        ))}
      </div>
      <Outlet />
    </div>
  );
}
