import { useEffect, useState } from "react";
import { AlertTriangle, CalendarOff, GraduationCap, HeartPulse, TrendingUp, Users } from "lucide-react";
import { useAuth } from "../auth/AuthContext";
import { getDashboardSummary } from "../api/dashboard";
import { extractErrorMessage } from "../api/client";
import { StatCard } from "../components/StatCard";
import { ProgressRing } from "../components/ProgressRing";
import { ModuleLaunchGrid } from "../components/ModuleLaunchGrid";
import { EmployeeRole } from "../types/auth";
import type { DashboardSummary } from "../types/dashboard";

function formatPercent(value: number | null): string {
  return value === null ? "—" : `${value}%`;
}

export function DashboardPage() {
  const { session } = useAuth();
  const firstName = session?.fullName.split(" ")[0] ?? "there";
  const isLineManager = session?.role === EmployeeRole.LineManager;

  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getDashboardSummary()
      .then(setSummary)
      .catch((err) => setError(extractErrorMessage(err)));
  }, []);

  const loadingDetail = error ?? "Loading...";

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h2 className="text-xl font-semibold text-slate-900">Welcome back, {firstName}</h2>
        <p className="text-sm text-slate-500">Here's what's happening today.</p>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
        <StatCard
          label="Attendance Today"
          value={summary ? `${summary.attendance.percentPresent}%` : "—"}
          detail={
            summary
              ? `${summary.attendance.presentCount} / ${summary.attendance.activeEmployeeCount} clocked in at your site`
              : loadingDetail
          }
          icon={Users}
          iconTone="blue"
          tone={summary && summary.attendance.percentPresent < 70 ? "warning" : "default"}
        />
        <StatCard
          label="Incidents (MTD)"
          value={summary ? String(summary.incidentsThisMonth) : "—"}
          detail={summary ? "Reported this month, org-wide" : loadingDetail}
          icon={AlertTriangle}
          iconTone={summary && summary.incidentsThisMonth > 0 ? "red" : "emerald"}
          tone={summary && summary.incidentsThisMonth > 0 ? "warning" : "good"}
        />
        <StatCard
          label="Medical Compliance"
          value={formatPercent(summary?.medicalCompliancePercent ?? null)}
          detail={summary ? "Employees with a current fit status" : loadingDetail}
          icon={HeartPulse}
          iconTone="violet"
          tone={
            summary?.medicalCompliancePercent != null && summary.medicalCompliancePercent < 80 ? "warning" : "default"
          }
        />
        <StatCard
          label="Training Compliance"
          value={formatPercent(summary?.trainingCompliancePercent ?? null)}
          detail={summary ? "Certificates not yet expired" : loadingDetail}
          icon={GraduationCap}
          iconTone="emerald"
          tone={
            summary?.trainingCompliancePercent != null && summary.trainingCompliancePercent < 80 ? "warning" : "default"
          }
        />
        <StatCard
          label="Leave Requests"
          value={summary ? String(summary.pendingLeaveCount) : "—"}
          detail={summary ? (isLineManager ? "Pending your approval" : "Your pending requests") : loadingDetail}
          icon={CalendarOff}
          iconTone="amber"
          tone={summary && summary.pendingLeaveCount > 0 ? "warning" : "default"}
        />
        <StatCard
          label="Performance"
          value={summary?.myAveragePerformanceScore != null ? `${summary.myAveragePerformanceScore} / 5` : "—"}
          detail={summary ? "Your average review score" : loadingDetail}
          icon={TrendingUp}
          iconTone="blue"
        />
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        <div className="flex flex-col items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <h3 className="self-start text-sm font-semibold text-slate-900">Attendance overview</h3>
          {summary ? (
            <ProgressRing
              percent={summary.attendance.percentPresent}
              label={`${summary.attendance.percentPresent}%`}
              sublabel="present today"
              progressClassName={summary.attendance.percentPresent < 70 ? "stroke-amber-500" : "stroke-emerald-500"}
            />
          ) : (
            <p className="py-10 text-sm text-slate-500">{loadingDetail}</p>
          )}
        </div>

        <div className="lg:col-span-2">
          <ModuleLaunchGrid />
        </div>
      </div>
    </div>
  );
}
