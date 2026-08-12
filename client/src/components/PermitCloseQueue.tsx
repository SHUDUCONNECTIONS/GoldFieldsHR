import { formatDate } from "../lib/format";
import { PermitTypeLabels, type WorkPermitDto } from "../types/permit";

interface PermitCloseQueueProps {
  items: WorkPermitDto[];
  isBusy: boolean;
  onClose: (id: string) => void;
}

export function PermitCloseQueue({ items, isBusy, onClose }: PermitCloseQueueProps) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-200 px-6 py-4">
        <h3 className="text-sm font-semibold text-slate-900">Open permits — awaiting close-out</h3>
      </div>
      {items.length === 0 ? (
        <p className="px-6 py-8 text-center text-sm text-slate-500">No open permits.</p>
      ) : (
        <ul className="divide-y divide-slate-100">
          {items.map((item) => (
            <li key={item.id} className="flex flex-wrap items-center justify-between gap-3 px-6 py-4">
              <div>
                <p className="text-sm font-medium text-slate-900">{item.employeeName}</p>
                <p className="text-sm text-slate-600">
                  {PermitTypeLabels[item.permitType]} — {item.location} — {formatDate(item.validFrom)} to{" "}
                  {formatDate(item.validTo)}
                </p>
              </div>
              <button
                type="button"
                disabled={isBusy}
                onClick={() => onClose(item.id)}
                className="rounded-md bg-red-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-red-500 disabled:opacity-50"
              >
                Close out
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
