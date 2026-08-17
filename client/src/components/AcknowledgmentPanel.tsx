import { useCallback, useEffect, useState } from "react";
import { CheckCircle2 } from "lucide-react";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessage } from "../api/client";
import { acknowledge, getAcknowledgmentsForEntity } from "../api/acknowledgments";
import { useToast } from "./ToastProvider";
import { formatDateTime } from "../lib/format";
import { EmployeeRole } from "../types/auth";
import type { AcknowledgmentDto, AcknowledgmentEntityType } from "../types/acknowledgment";

interface AcknowledgmentPanelProps {
  entityType: AcknowledgmentEntityType;
  entityId: string;
  compact?: boolean;
}

export function AcknowledgmentPanel({ entityType, entityId, compact = false }: AcknowledgmentPanelProps) {
  const { session } = useAuth();
  const { showError } = useToast();
  const canAcknowledge = session?.role === EmployeeRole.HR || session?.role === EmployeeRole.Executive;

  const [acknowledgments, setAcknowledgments] = useState<AcknowledgmentDto[]>([]);
  const [visible, setVisible] = useState(true);
  const [isAcknowledging, setIsAcknowledging] = useState(false);

  const load = useCallback(async () => {
    try {
      const data = await getAcknowledgmentsForEntity(entityType, entityId);
      setAcknowledgments(data);
    } catch {
      // A 400 here means the viewer isn't authorized to see this record's acknowledgments
      // (e.g. a plain Employee) — hide the panel entirely rather than show a confusing error.
      setVisible(false);
    }
  }, [entityType, entityId]);

  useEffect(() => {
    load();
  }, [load]);

  async function handleAcknowledge() {
    setIsAcknowledging(true);
    try {
      await acknowledge(entityType, entityId);
      await load();
    } catch (err) {
      showError(extractErrorMessage(err));
    } finally {
      setIsAcknowledging(false);
    }
  }

  if (!visible) return null;

  const hasAcknowledged = acknowledgments.some((a) => a.employeeId === session?.employeeId);

  return (
    <div className={compact ? "" : "mt-3 border-t border-slate-100 pt-3"}>
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex flex-wrap items-center gap-1.5 text-xs text-slate-500">
          <CheckCircle2 className="h-3.5 w-3.5 shrink-0" />
          {acknowledgments.length === 0 ? (
            <span>Not yet acknowledged by HR/Executive.</span>
          ) : (
            <span>
              Acknowledged by{" "}
              {acknowledgments
                .map((a) => `${a.employeeName} (${formatDateTime(a.createdAtUtc)})`)
                .join(", ")}
            </span>
          )}
        </div>
        {canAcknowledge && !hasAcknowledged && (
          <button
            type="button"
            disabled={isAcknowledging}
            onClick={handleAcknowledge}
            className="shrink-0 rounded-md border border-yellow-600 px-2.5 py-1 text-xs font-medium text-yellow-700 hover:bg-yellow-50 disabled:opacity-50"
          >
            {isAcknowledging ? "Acknowledging..." : "Acknowledge"}
          </button>
        )}
      </div>
    </div>
  );
}
