import { PpeItemTypeLabels, type PpeRequestDto } from "../types/ppe";

interface PpeIssueQueueProps {
  items: PpeRequestDto[];
  isBusy: boolean;
  onIssue: (id: string) => void;
}

export function PpeIssueQueue({ items, isBusy, onIssue }: PpeIssueQueueProps) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-200 px-6 py-4">
        <h3 className="text-sm font-semibold text-slate-900">Approved — awaiting issue</h3>
      </div>
      {items.length === 0 ? (
        <p className="px-6 py-8 text-center text-sm text-slate-500">Nothing awaiting issue.</p>
      ) : (
        <ul className="divide-y divide-slate-100">
          {items.map((item) => (
            <li key={item.id} className="flex flex-wrap items-center justify-between gap-3 px-6 py-4">
              <div>
                <p className="text-sm font-medium text-slate-900">{item.employeeName}</p>
                <p className="text-sm text-slate-600">
                  {PpeItemTypeLabels[item.itemType]} × {item.quantity}
                  {item.size ? ` (size ${item.size})` : ""}
                </p>
              </div>
              <button
                type="button"
                disabled={isBusy}
                onClick={() => onIssue(item.id)}
                className="rounded-md bg-red-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-red-500 disabled:opacity-50"
              >
                Mark issued
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
