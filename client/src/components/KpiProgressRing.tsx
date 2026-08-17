// CSS-only donut ring: a radial-gradient punches a hole out of a conic-gradient
// arc, so no SVG is needed. Direct port of Collabrio's ProgressCircle.jsx.
function ringColor(percent: number | null): string {
  if (percent === null) return "#8e9195"; // grey[500] — unscored
  if (percent >= 80) return "#1ecb8f"; // greenAccent 500
  if (percent >= 50) return "#f5a83c"; // orangeAccent 500
  return "#c65a5a"; // redAccent 500
}

interface KpiProgressRingProps {
  percent: number | null;
  size?: number;
}

export function KpiProgressRing({ percent, size = 56 }: KpiProgressRingProps) {
  const angle = ((percent ?? 0) / 100) * 360;
  const color = ringColor(percent);

  return (
    <div
      className="relative shrink-0 rounded-full"
      style={{
        width: size,
        height: size,
        background: `radial-gradient(#2d2f31 63%, transparent 64%), conic-gradient(${color} ${angle}deg, rgba(255,255,255,0.1) ${angle}deg 360deg)`,
      }}
    >
      <span className="absolute inset-0 flex items-center justify-center text-[11px] font-semibold text-white">
        {percent === null ? "—" : `${percent}%`}
      </span>
    </div>
  );
}
