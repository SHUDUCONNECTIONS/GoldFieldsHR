const PALETTE = ["#6fbe44", "#1ecb8f", "#c65a5a", "#f5a83c", "#ffc107", "#549938"];

function colorForName(name: string): string {
  let hash = 0;
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash);
  }
  return PALETTE[Math.abs(hash) % PALETTE.length];
}

function initials(name: string): string {
  const parts = name.trim().split(/\s+/);
  return ((parts[0]?.[0] ?? "") + (parts[1]?.[0] ?? "")).toUpperCase() || "?";
}

interface PersonAvatarProps {
  name: string;
  size?: number;
  className?: string;
}

export function PersonAvatar({ name, size = 28, className = "" }: PersonAvatarProps) {
  return (
    <span
      className={`flex shrink-0 items-center justify-center rounded-full border-2 border-white/10 font-semibold text-white ${className}`}
      style={{ width: size, height: size, fontSize: size * 0.4, backgroundColor: colorForName(name) }}
      title={name}
    >
      {initials(name)}
    </span>
  );
}
