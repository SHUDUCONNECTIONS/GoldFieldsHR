export type BadgeTone = "amber" | "emerald" | "red";

const toneClasses: Record<BadgeTone, string> = {
  amber: "bg-amber-100 text-amber-800",
  emerald: "bg-emerald-100 text-emerald-800",
  red: "bg-red-100 text-red-800",
};

export function Badge({ label, tone }: { label: string; tone: BadgeTone }) {
  return (
    <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${toneClasses[tone]}`}>{label}</span>
  );
}
