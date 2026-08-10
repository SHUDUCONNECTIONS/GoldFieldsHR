import type { LucideIcon } from "lucide-react";

interface StatCardProps {
  label: string;
  value: string;
  detail: string;
  tone?: "default" | "warning" | "good";
  icon?: LucideIcon;
  iconTone?: "blue" | "amber" | "emerald" | "red" | "violet";
}

const toneClasses: Record<NonNullable<StatCardProps["tone"]>, string> = {
  default: "text-slate-900",
  warning: "text-amber-600",
  good: "text-emerald-600",
};

const iconToneClasses: Record<NonNullable<StatCardProps["iconTone"]>, string> = {
  blue: "bg-blue-50 text-blue-600",
  amber: "bg-amber-50 text-amber-600",
  emerald: "bg-emerald-50 text-emerald-600",
  red: "bg-red-50 text-red-600",
  violet: "bg-violet-50 text-violet-600",
};

export function StatCard({ label, value, detail, tone = "default", icon: Icon, iconTone = "blue" }: StatCardProps) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
      <div className="flex items-start justify-between gap-2">
        <p className="text-xs font-medium uppercase tracking-wide text-slate-500">{label}</p>
        {Icon && (
          <span className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-lg ${iconToneClasses[iconTone]}`}>
            <Icon className="h-4 w-4" />
          </span>
        )}
      </div>
      <p className={`mt-2 text-2xl font-semibold ${toneClasses[tone]}`}>{value}</p>
      <p className="mt-1 text-xs text-slate-500">{detail}</p>
    </div>
  );
}
