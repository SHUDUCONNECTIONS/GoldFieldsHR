import { useEffect, useState } from "react";
import { Check } from "lucide-react";
import { getEmployeeDirectoryLite } from "../../api/employees";
import { PersonAvatar } from "./PersonAvatar";
import type { EmployeeLiteDto } from "../../types/board";

interface BoardMemberPickerProps {
  selectedIds: string[];
  onChange: (ids: string[]) => void;
  /** Excluded from the pickable list — e.g. the current user (added automatically) or existing board members. */
  excludeEmployeeIds?: string[];
}

export function BoardMemberPicker({ selectedIds, onChange, excludeEmployeeIds = [] }: BoardMemberPickerProps) {
  const [employees, setEmployees] = useState<EmployeeLiteDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    getEmployeeDirectoryLite()
      .then(setEmployees)
      .finally(() => setIsLoading(false));
  }, []);

  function toggle(id: string) {
    onChange(selectedIds.includes(id) ? selectedIds.filter((existing) => existing !== id) : [...selectedIds, id]);
  }

  const pickable = employees.filter((e) => !excludeEmployeeIds.includes(e.id));

  if (isLoading) {
    return <p className="text-sm text-white/40">Loading people...</p>;
  }

  if (pickable.length === 0) {
    return <p className="text-sm text-white/40">No other employees found.</p>;
  }

  return (
    <div className="flex flex-wrap gap-2">
      {pickable.map((employee) => {
        const isSelected = selectedIds.includes(employee.id);
        return (
          <button
            key={employee.id}
            type="button"
            onClick={() => toggle(employee.id)}
            title={employee.jobTitle}
            className={`flex items-center gap-1.5 rounded-full border px-2 py-1 text-xs font-medium transition-all ${
              isSelected
                ? "scale-[1.04] border-[#6fbe44] bg-[#6fbe44]/15 text-[#93d75f]"
                : "border-white/15 bg-white/5 text-white/70 hover:border-white/30"
            }`}
          >
            <span className="relative">
              <PersonAvatar name={employee.fullName} size={22} />
              {isSelected && (
                <span className="absolute -bottom-0.5 -right-0.5 flex h-3.5 w-3.5 items-center justify-center rounded-full border border-[#202325] bg-[#6fbe44]">
                  <Check className="h-2 w-2 text-[#131415]" />
                </span>
              )}
            </span>
            {employee.fullName.split(" ")[0]}
          </button>
        );
      })}
    </div>
  );
}
