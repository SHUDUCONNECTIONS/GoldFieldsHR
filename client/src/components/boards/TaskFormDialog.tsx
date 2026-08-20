import { useState, type FormEvent } from "react";
import type { BoardMemberDto, BoardTaskDto } from "../../types/board";

const inputClasses =
  "rounded-lg border border-white/10 bg-[#202325] px-3 py-2 text-sm text-white placeholder:text-white/30 focus:border-[#6fbe44] focus:outline-none focus:ring-2 focus:ring-[#6fbe44]/20";
const labelClasses = "flex flex-col gap-1 text-sm text-white/70";

export interface TaskFormValues {
  title: string;
  description: string;
  assigneeEmployeeId: string;
  dueDate: string;
}

interface TaskFormDialogProps {
  members: BoardMemberDto[];
  task?: BoardTaskDto;
  isSubmitting: boolean;
  error: string | null;
  onSubmit: (values: TaskFormValues) => void;
  onClose: () => void;
}

export function TaskFormDialog({ members, task, isSubmitting, error, onSubmit, onClose }: TaskFormDialogProps) {
  const [values, setValues] = useState<TaskFormValues>({
    title: task?.title ?? "",
    description: task?.description ?? "",
    assigneeEmployeeId: task?.assigneeEmployeeId ?? "",
    dueDate: task?.dueDate ?? "",
  });

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!values.title.trim()) return;
    onSubmit(values);
  }

  return (
    <div className="fixed inset-0 z-[90] flex items-center justify-center bg-slate-950/60 px-4">
      <div className="w-full max-w-md rounded-2xl border border-white/10 bg-[#3a3d40] p-6 shadow-2xl font-['Inter']">
        <h3 className="mb-4 text-sm font-semibold text-white">{task ? "Edit task" : "New task"}</h3>
        <form onSubmit={handleSubmit} className="flex flex-col gap-3">
          <label className={labelClasses}>
            Title
            <input
              required
              autoFocus
              value={values.title}
              onChange={(e) => setValues((prev) => ({ ...prev, title: e.target.value }))}
              className={inputClasses}
            />
          </label>
          <label className={labelClasses}>
            Description (optional)
            <textarea
              rows={2}
              value={values.description}
              onChange={(e) => setValues((prev) => ({ ...prev, description: e.target.value }))}
              className={inputClasses}
            />
          </label>
          <label className={labelClasses}>
            Assign to
            <select
              value={values.assigneeEmployeeId}
              onChange={(e) => setValues((prev) => ({ ...prev, assigneeEmployeeId: e.target.value }))}
              className={inputClasses}
            >
              <option value="">Unassigned</option>
              {members.map((member) => (
                <option key={member.employeeId} value={member.employeeId}>
                  {member.employeeName}
                </option>
              ))}
            </select>
          </label>
          <label className={labelClasses}>
            Due date (optional)
            <input
              type="date"
              value={values.dueDate}
              onChange={(e) => setValues((prev) => ({ ...prev, dueDate: e.target.value }))}
              className={inputClasses}
            />
          </label>

          {error && <p className="text-sm text-[#e69c9c]">{error}</p>}

          <div className="mt-1 flex items-center gap-2">
            <button
              type="submit"
              disabled={isSubmitting}
              className="rounded-lg bg-[#6fbe44] px-4 py-2 text-sm font-semibold text-[#131415] transition-colors hover:bg-[#93d75f] disabled:opacity-50"
            >
              {isSubmitting ? "Saving..." : task ? "Save changes" : "Create task"}
            </button>
            <button
              type="button"
              onClick={onClose}
              className="text-xs text-white/40 hover:text-white/70 hover:underline"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
