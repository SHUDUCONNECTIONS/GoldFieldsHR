import { Badge, type BadgeTone } from "./Badge";

export function ScoreBadge({ score }: { score: number }) {
  const tone: BadgeTone = score >= 4 ? "emerald" : score === 3 ? "amber" : "red";
  return <Badge label={`${score} / 5`} tone={tone} />;
}
