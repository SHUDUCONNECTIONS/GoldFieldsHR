export type BadgeTone = "amber" | "emerald" | "red";

const toneClasses: Record<BadgeTone, string> = {
  amber: "bg-amber-50 text-amber-800 ring-1 ring-inset ring-amber-300/60",
  emerald: "bg-emerald-50 text-emerald-800 ring-1 ring-inset ring-emerald-300/60",
  red: "bg-red-50 text-red-800 ring-1 ring-inset ring-red-300/60",
};

export function Badge({ label, tone }: { label: string; tone: BadgeTone }) {
  return (
    <span className={`inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs font-medium ${toneClasses[tone]}`}>
      <span className="h-1.5 w-1.5 rounded-full bg-current opacity-70" />
      {label}
    </span>
  );
}
