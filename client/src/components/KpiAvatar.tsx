function initials(name: string): string {
  const parts = name.trim().split(/\s+/);
  return ((parts[0]?.[0] ?? "") + (parts[1]?.[0] ?? "")).toUpperCase() || "?";
}

interface KpiAvatarProps {
  name: string;
  size?: number;
}

export function KpiAvatar({ name, size = 32 }: KpiAvatarProps) {
  return (
    <span
      className="flex shrink-0 items-center justify-center rounded-full border-2 border-[#6fbe44] bg-[#549938] font-semibold text-white"
      style={{ width: size, height: size, fontSize: size * 0.4 }}
    >
      {initials(name)}
    </span>
  );
}
