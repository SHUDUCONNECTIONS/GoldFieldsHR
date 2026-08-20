import { CalendarDays, CheckCircle2, Pencil, Trash2 } from "lucide-react";
import { PersonAvatar } from "./PersonAvatar";
import { formatDate } from "../../lib/format";
import { BoardTaskStatus, type BoardTaskDto } from "../../types/board";

function isOverdue(task: BoardTaskDto): boolean {
  if (!task.dueDate || task.status === BoardTaskStatus.Done) return false;
  return task.dueDate < new Date().toISOString().slice(0, 10);
}

interface TaskCardProps {
  task: BoardTaskDto;
  /** Any board member — matches the backend's ChangeStatusAsync/UpdateAsync rules. */
  canChangeStatus: boolean;
  /** Any board member — matches UpdateAsync. */
  canEdit: boolean;
  /** Owner only — matches DeleteAsync. */
  canDelete: boolean;
  onMoveStatus: (status: BoardTaskStatus) => void;
  onEdit: () => void;
  onDelete: () => void;
}

export function TaskCard({ task, canChangeStatus, canEdit, canDelete, onMoveStatus, onEdit, onDelete }: TaskCardProps) {
  const overdue = isOverdue(task);
  const isDone = task.status === BoardTaskStatus.Done;

  return (
    <div
      className={`group flex items-start gap-2 rounded-lg border bg-[#2d2f31] p-3 transition-all duration-150 hover:-translate-y-0.5 hover:shadow-lg ${
        isDone ? "border-[#1ecb8f]/30 border-l-2 border-l-[#1ecb8f]" : "border-white/5 hover:border-[#6fbe44]/30"
      }`}
    >
      <div className="min-w-0 flex-1">
        <p className={`text-sm ${isDone ? "text-white/50 line-through" : "text-white/90"}`}>{task.title}</p>
        {task.description && <p className="mt-1 line-clamp-2 text-xs text-white/50">{task.description}</p>}

        <div className="mt-2 flex flex-wrap items-center gap-1.5">
          {task.assigneeEmployeeName && (
            <span className="flex items-center gap-1 rounded-full bg-white/5 py-0.5 pl-0.5 pr-2 text-[10px] font-medium text-white/70">
              <PersonAvatar name={task.assigneeEmployeeName} size={16} />
              {task.assigneeEmployeeName.split(" ")[0]}
            </span>
          )}
          {task.dueDate && (
            <span
              className={`flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-medium ${
                overdue ? "bg-[#c65a5a]/15 text-[#e69c9c]" : "bg-white/5 text-white/50"
              }`}
            >
              <CalendarDays className="h-2.5 w-2.5" />
              {formatDate(task.dueDate)}
            </span>
          )}
        </div>

        {isDone ? (
          <div className="mt-2 flex items-center gap-1 text-[11px] font-semibold text-[#1ecb8f]">
            <CheckCircle2 className="h-3.5 w-3.5" />
            Completed
          </div>
        ) : (
          canChangeStatus && (
            <div className="mt-2 flex flex-wrap items-center gap-1.5">
              {task.status === BoardTaskStatus.Todo && (
                <button
                  type="button"
                  onClick={() => onMoveStatus(BoardTaskStatus.InProgress)}
                  className="rounded-full border border-[#f5a83c]/40 px-2 py-0.5 text-[10px] font-medium text-[#f5a83c] transition-colors hover:bg-[#f5a83c]/10"
                >
                  In Progress
                </button>
              )}
              <button
                type="button"
                onClick={() => onMoveStatus(BoardTaskStatus.Done)}
                className="rounded-full border border-[#1ecb8f]/40 px-2 py-0.5 text-[10px] font-medium text-[#1ecb8f] transition-colors hover:bg-[#1ecb8f]/10"
              >
                Completed
              </button>
            </div>
          )
        )}
      </div>

      {(canEdit || canDelete) && (
        <div className="flex shrink-0 flex-col gap-1 opacity-0 transition-opacity group-hover:opacity-100">
          {canEdit && (
            <button
              type="button"
              onClick={onEdit}
              aria-label="Edit task"
              className="rounded p-1 text-white/40 hover:bg-white/10 hover:text-white"
            >
              <Pencil className="h-3 w-3" />
            </button>
          )}
          {canDelete && (
            <button
              type="button"
              onClick={onDelete}
              aria-label="Delete task"
              className="rounded p-1 text-white/40 hover:bg-[#c65a5a]/20 hover:text-[#e69c9c]"
            >
              <Trash2 className="h-3 w-3" />
            </button>
          )}
        </div>
      )}
    </div>
  );
}
