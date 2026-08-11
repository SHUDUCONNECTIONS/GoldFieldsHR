import { EmployeeRoleLabels } from "../types/auth";
import type { EmployeeSummaryDto } from "../types/employee";

interface RoleRequestQueueProps {
  items: EmployeeSummaryDto[];
  isBusy: boolean;
  onGrant: (employee: EmployeeSummaryDto) => void;
  onDismiss: (employee: EmployeeSummaryDto) => void;
}

export function RoleRequestQueue({ items, isBusy, onGrant, onDismiss }: RoleRequestQueueProps) {
  if (items.length === 0) {
    return null;
  }

  return (
    <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-200 px-6 py-4">
        <h3 className="text-sm font-semibold text-slate-900">Pending role requests</h3>
      </div>
      <ul className="divide-y divide-slate-100">
        {items.map((employee) => (
          <li key={employee.id} className="flex flex-wrap items-start justify-between gap-3 px-6 py-4">
            <div>
              <p className="text-sm font-medium text-slate-900">{employee.fullName}</p>
              <p className="text-sm text-slate-600">
                {employee.jobTitle} — currently {EmployeeRoleLabels[employee.role]}
              </p>
              <p className="mt-1 text-xs text-slate-500">
                Requested: <span className="font-medium text-amber-600">{EmployeeRoleLabels[employee.requestedRole!]}</span>
              </p>
            </div>
            <div className="flex gap-2">
              <button
                type="button"
                disabled={isBusy}
                onClick={() => onGrant(employee)}
                className="rounded-md bg-emerald-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
              >
                Grant
              </button>
              <button
                type="button"
                disabled={isBusy}
                onClick={() => onDismiss(employee)}
                className="rounded-md border border-slate-300 px-3 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-50"
              >
                Dismiss
              </button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
