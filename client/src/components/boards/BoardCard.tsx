import { Link } from "react-router-dom";
import { CalendarDays, Users } from "lucide-react";
import { PersonAvatar } from "./PersonAvatar";
import { formatDate } from "../../lib/format";
import {
  BoardPriority,
  BoardPriorityColors,
  BoardPriorityLabels,
  BoardStatus,
  BoardStatusLabels,
  type BoardDto,
} from "../../types/board";

const MAX_VISIBLE_MEMBERS = 5;

const STATUS_COLORS: Record<BoardStatus, string> = {
  [BoardStatus.NotStarted]: "#8e9195",
  [BoardStatus.InProgress]: "#f5a83c",
  [BoardStatus.OnHold]: "#c65a5a",
  [BoardStatus.Completed]: "#1ecb8f",
};

interface BoardCardProps {
  board: BoardDto;
}

export function BoardCard({ board }: BoardCardProps) {
  const visibleMembers = board.members.slice(0, MAX_VISIBLE_MEMBERS);
  const overflowCount = board.members.length - visibleMembers.length;
  const statusColor = STATUS_COLORS[board.status];

  return (
    <Link
      to={`/kpi/boards/${board.id}`}
      className="flex flex-col gap-3 rounded-xl border-l-4 border-l-[#6fbe44] bg-[#3a3d40] p-4 shadow-lg transition-all duration-200 hover:-translate-y-1 hover:shadow-2xl"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="truncate text-sm font-semibold text-white">{board.name}</p>
          <p className="truncate text-xs text-white/50">Owned by {board.ownerEmployeeName}</p>
        </div>
        <div className="flex shrink-0 flex-col items-end gap-1">
          {board.isArchived && (
            <span className="rounded-full bg-white/10 px-2 py-0.5 text-[10px] font-semibold text-white/60">
              Archived
            </span>
          )}
          <span
            className="rounded-full px-2 py-0.5 text-[10px] font-semibold"
            style={{ backgroundColor: `${statusColor}26`, color: statusColor }}
          >
            {BoardStatusLabels[board.status]}
          </span>
        </div>
      </div>

      {board.description && <p className="line-clamp-2 text-xs text-white/60">{board.description}</p>}

      <div className="flex flex-wrap items-center gap-1.5">
        {board.priority !== BoardPriority.Normal && (
          <span
            className="rounded-full px-2 py-0.5 text-[10px] font-semibold"
            style={{ backgroundColor: `${BoardPriorityColors[board.priority]}26`, color: BoardPriorityColors[board.priority] }}
          >
            {BoardPriorityLabels[board.priority]}
          </span>
        )}
        {board.deadline && (
          <span className="flex items-center gap-1 rounded-full bg-white/5 px-2 py-0.5 text-[10px] font-medium text-white/50">
            <CalendarDays className="h-2.5 w-2.5" />
            {formatDate(board.deadline)}
          </span>
        )}
      </div>

      <div className="flex items-center gap-2">
        <div className="h-1.5 flex-1 overflow-hidden rounded-full bg-white/10">
          <div className="h-full rounded-full bg-[#6fbe44] transition-all" style={{ width: `${board.completionPercentage}%` }} />
        </div>
        <span className="text-[10px] font-semibold text-white/50">{board.completionPercentage}%</span>
      </div>

      <div className="mt-auto flex items-center gap-2 pt-1">
        <Users className="h-3.5 w-3.5 text-white/40" />
        <div className="flex -space-x-2">
          {visibleMembers.map((member) => (
            <PersonAvatar key={member.employeeId} name={member.employeeName} size={24} className="border-[#3a3d40]" />
          ))}
          {overflowCount > 0 && (
            <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full border-2 border-[#3a3d40] bg-white/10 text-[10px] font-semibold text-white/70">
              +{overflowCount}
            </span>
          )}
        </div>
      </div>
    </Link>
  );
}
