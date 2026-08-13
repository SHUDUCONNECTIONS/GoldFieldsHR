import { useEffect, useState } from "react";
import { AlertTriangle, Award, CalendarOff, Download, HardHat, Scale, Users } from "lucide-react";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessage } from "../api/client";
import { getReportsSummary } from "../api/reports";
import { StatCard } from "../components/StatCard";
import { useToast } from "../components/ToastProvider";
import { downloadCsv } from "../lib/csv";
import { EmployeeRole, EmployeeRoleLabels } from "../types/auth";
import { IncidentSeverityLabels } from "../types/incident";
import type { ReportsSummaryDto } from "../types/reports";

const ALLOWED_ROLES: EmployeeRole[] = [EmployeeRole.HR, EmployeeRole.SafetyOfficer, EmployeeRole.Executive];

export function ReportsPage() {
  const { session } = useAuth();
  const { showSuccess } = useToast();
  const isAllowed = session ? ALLOWED_ROLES.includes(session.role) : false;

  const [summary, setSummary] = useState<ReportsSummaryDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isAllowed) return;
    getReportsSummary()
      .then(setSummary)
      .catch((err) => setError(extractErrorMessage(err)));
  }, [isAllowed]);

  if (!isAllowed) {
    return (
      <div className="rounded-lg border border-slate-200 bg-white p-8 text-center shadow-sm">
        <p className="text-sm text-slate-600">
          Reports &amp; Analytics is available to HR, Safety Officer, and Executive roles.
        </p>
      </div>
    );
  }

  if (error) {
    return <p className="text-sm text-red-600">{error}</p>;
  }

  if (!summary) {
    return <p className="text-sm text-slate-500">Loading...</p>;
  }

  function handleExportCsv() {
    if (!summary) return;
    downloadCsv(`reports-summary-${new Date().toISOString().slice(0, 10)}.csv`, ["Metric", "Value"], [
      ["Attendance today (present / active)", `${summary.attendanceToday.presentCount} / ${summary.attendanceToday.activeEmployeeCount}`],
      ["Attendance today (%)", summary.attendanceToday.percentPresent],
      ["Active employees", summary.activeEmployees],
      ["Total employees", summary.totalEmployees],
      ["Open incidents", summary.openIncidents],
      ["Closed incidents", summary.closedIncidents],
      ["Pending leave requests", summary.pendingLeaveRequests],
      ["Valid certificates", summary.validCertificates],
      ["Due soon certificates", summary.dueSoonCertificates],
      ["Expired certificates", summary.expiredCertificates],
      ["Pending PPE requests", summary.pendingPpeRequests],
      ["PPE awaiting issue", summary.ppeAwaitingIssue],
      ["Pending legal appointments", summary.pendingLegalAppointments],
      ["Active legal appointments", summary.activeLegalAppointments],
      ...summary.headcountByRole.map((r) => [`Headcount — ${EmployeeRoleLabels[r.role]}`, r.count]),
      ...summary.openIncidentsBySeverity.map((s) => [`Open incidents — ${IncidentSeverityLabels[s.severity]}`, s.count]),
    ]);
    showSuccess("Reports summary exported to CSV.");
  }

  return (
    <div className="stagger-children flex flex-col gap-6">
      <div className="flex justify-end">
        <button
          type="button"
          onClick={handleExportCsv}
          className="flex items-center gap-1.5 rounded-md border border-slate-300 bg-white px-3 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-50"
        >
          <Download className="h-3.5 w-3.5" />
          Export CSV
        </button>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard
          label="Attendance today"
          value={`${summary.attendanceToday.presentCount} / ${summary.attendanceToday.activeEmployeeCount}`}
          detail={`${summary.attendanceToday.percentPresent}% present`}
          icon={Users}
          iconTone="blue"
        />
        <StatCard
          label="Active employees"
          value={String(summary.activeEmployees)}
          detail={`${summary.totalEmployees} total on record`}
          icon={Users}
          iconTone="blue"
        />
        <StatCard
          label="Open incidents"
          value={String(summary.openIncidents)}
          detail={`${summary.closedIncidents} closed`}
          tone={summary.openIncidents > 0 ? "warning" : "good"}
          icon={AlertTriangle}
          iconTone={summary.openIncidents > 0 ? "red" : "emerald"}
        />
        <StatCard
          label="Pending leave requests"
          value={String(summary.pendingLeaveRequests)}
          detail="Awaiting Line Manager review"
          tone={summary.pendingLeaveRequests > 0 ? "warning" : "good"}
          icon={CalendarOff}
          iconTone="amber"
        />
        <StatCard
          label="Certificates expired"
          value={String(summary.expiredCertificates)}
          detail={`${summary.dueSoonCertificates} due soon, ${summary.validCertificates} valid`}
          tone={summary.expiredCertificates > 0 ? "warning" : "good"}
          icon={Award}
          iconTone={summary.expiredCertificates > 0 ? "red" : "emerald"}
        />
        <StatCard
          label="PPE pending"
          value={String(summary.pendingPpeRequests)}
          detail={`${summary.ppeAwaitingIssue} approved, awaiting issue`}
          icon={HardHat}
          iconTone="violet"
        />
        <StatCard
          label="Legal appointments pending"
          value={String(summary.pendingLegalAppointments)}
          detail={`${summary.activeLegalAppointments} active`}
          icon={Scale}
          iconTone="violet"
        />
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
          <div className="border-b border-slate-200 px-6 py-4">
            <h3 className="text-sm font-semibold text-slate-900">Headcount by role</h3>
          </div>
          {summary.headcountByRole.length === 0 ? (
            <p className="px-6 py-8 text-center text-sm text-slate-500">No employees on record.</p>
          ) : (
            <ul className="divide-y divide-slate-100">
              {summary.headcountByRole.map((item) => (
                <li key={item.role} className="flex items-center justify-between px-6 py-3 text-sm">
                  <span className="text-slate-700">{EmployeeRoleLabels[item.role]}</span>
                  <span className="font-medium text-slate-900">{item.count}</span>
                </li>
              ))}
            </ul>
          )}
        </div>

        <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
          <div className="border-b border-slate-200 px-6 py-4">
            <h3 className="text-sm font-semibold text-slate-900">Open incidents by severity</h3>
          </div>
          {summary.openIncidentsBySeverity.length === 0 ? (
            <p className="px-6 py-8 text-center text-sm text-slate-500">No open incidents.</p>
          ) : (
            <ul className="divide-y divide-slate-100">
              {summary.openIncidentsBySeverity.map((item) => (
                <li key={item.severity} className="flex items-center justify-between px-6 py-3 text-sm">
                  <span className="text-slate-700">{IncidentSeverityLabels[item.severity]}</span>
                  <span className="font-medium text-slate-900">{item.count}</span>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  );
}
