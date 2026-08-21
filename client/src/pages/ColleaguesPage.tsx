import { useCallback, useEffect, useState } from "react";
import { ChevronLeft, ChevronRight, Download, Search, ShieldCheck, ShieldX, UserCog } from "lucide-react";
import { useAuth } from "../auth/AuthContext";
import { dismissRequestedRole, getEmployees, setEmployeeActiveStatus, setEmployeeRole } from "../api/employees";
import { downloadEmployeePerformancePdf } from "../api/performance";
import { extractErrorMessage } from "../api/client";
import { PersonAvatar } from "../components/boards/PersonAvatar";
import { KpiSkeletonCard } from "../components/KpiSkeletons";
import { ConfirmDialog } from "../components/ConfirmDialog";
import { EmployeeRole, EmployeeRoleLabels } from "../types/auth";
import type { EmployeeSummaryDto } from "../types/employee";

const PAGE_SIZE = 12;
const ASSIGNABLE_ROLES = Object.values(EmployeeRole).filter((v): v is EmployeeRole => typeof v === "number");

export function ColleaguesPage() {
  const { session } = useAuth();
  const isHR = session?.role === EmployeeRole.HR;

  const [employees, setEmployees] = useState<EmployeeSummaryDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [generatingId, setGeneratingId] = useState<string | null>(null);

  const [roleTarget, setRoleTarget] = useState<EmployeeSummaryDto | null>(null);
  const [pendingRole, setPendingRole] = useState<EmployeeRole>(EmployeeRole.Employee);
  const [isSavingRole, setIsSavingRole] = useState(false);
  const [roleError, setRoleError] = useState<string | null>(null);

  const [statusTarget, setStatusTarget] = useState<EmployeeSummaryDto | null>(null);
  const [isSavingStatus, setIsSavingStatus] = useState(false);

  const load = useCallback(async () => {
    setIsLoading(true);
    try {
      const result = await getEmployees({ search: search || undefined, page, pageSize: PAGE_SIZE });
      setEmployees(result.items);
      setTotalCount(result.totalCount);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  }, [search, page]);

  useEffect(() => {
    load();
  }, [load]);

  async function handleDownloadPdf(employee: EmployeeSummaryDto) {
    setGeneratingId(employee.id);
    try {
      await downloadEmployeePerformancePdf(employee.id, employee.fullName);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setGeneratingId(null);
    }
  }

  function openRoleEditor(employee: EmployeeSummaryDto) {
    setPendingRole(employee.role);
    setRoleError(null);
    setRoleTarget(employee);
  }

  async function handleSaveRole() {
    if (!roleTarget) return;
    setIsSavingRole(true);
    setRoleError(null);
    try {
      const updated = await setEmployeeRole(roleTarget.id, { role: pendingRole });
      setEmployees((prev) => prev.map((e) => (e.id === updated.id ? updated : e)));
      setRoleTarget(null);
    } catch (err) {
      setRoleError(extractErrorMessage(err));
    } finally {
      setIsSavingRole(false);
    }
  }

  async function handleConfirmStatus() {
    if (!statusTarget) return;
    setIsSavingStatus(true);
    try {
      const updated = await setEmployeeActiveStatus(statusTarget.id, { isActive: !statusTarget.isActive });
      setEmployees((prev) => prev.map((e) => (e.id === updated.id ? updated : e)));
      setStatusTarget(null);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSavingStatus(false);
    }
  }

  async function handleDismissRequestedRole(employee: EmployeeSummaryDto) {
    try {
      const updated = await dismissRequestedRole(employee.id);
      setEmployees((prev) => prev.map((e) => (e.id === updated.id ? updated : e)));
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }

  async function handleApproveRequestedRole(employee: EmployeeSummaryDto) {
    if (!employee.requestedRole) return;
    try {
      const updated = await setEmployeeRole(employee.id, { role: employee.requestedRole });
      setEmployees((prev) => prev.map((e) => (e.id === updated.id ? updated : e)));
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }

  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  return (
    <div className="stagger-children flex flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-white">Colleagues</h2>
        <div className="relative w-full max-w-xs">
          <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-white/30" />
          <input
            value={search}
            onChange={(e) => {
              setPage(1);
              setSearch(e.target.value);
            }}
            placeholder="Search colleagues..."
            className="w-full rounded-lg border border-white/10 bg-[#3a3d40] py-2 pl-9 pr-3 text-sm text-white placeholder:text-white/30 focus:border-[#6fbe44] focus:outline-none focus:ring-2 focus:ring-[#6fbe44]/20"
          />
        </div>
      </div>

      {error && <p className="text-sm text-[#e69c9c]">{error}</p>}

      {isLoading ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          <KpiSkeletonCard />
          <KpiSkeletonCard />
          <KpiSkeletonCard />
          <KpiSkeletonCard />
        </div>
      ) : employees.length === 0 ? (
        <p className="rounded-xl border border-white/10 bg-[#3a3d40] px-6 py-8 text-center text-sm text-white/40">
          No colleagues found.
        </p>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {employees.map((employee) => (
            <div
              key={employee.id}
              className={`flex flex-col gap-3 rounded-xl border-l-4 bg-[#3a3d40] p-4 shadow-lg ${
                employee.isActive ? "border-l-[#6fbe44]" : "border-l-white/20 opacity-60"
              }`}
            >
              <div className="flex items-start justify-between gap-2">
                <div className="flex items-center gap-2.5">
                  <PersonAvatar name={employee.fullName} size={40} />
                  <div className="min-w-0">
                    <p className="truncate text-sm font-semibold text-white">{employee.fullName}</p>
                    <p className="truncate text-xs text-white/50">{employee.jobTitle}</p>
                  </div>
                </div>
                {!employee.isActive && (
                  <span className="shrink-0 rounded-full bg-white/10 px-2 py-0.5 text-[10px] font-semibold text-white/60">
                    Inactive
                  </span>
                )}
              </div>

              <p className="text-[11px] text-white/40">{employee.siteName}</p>

              <button
                type="button"
                onClick={() => (isHR || session?.role === EmployeeRole.Executive) && openRoleEditor(employee)}
                className="w-fit rounded-full bg-[#6fbe44]/15 px-2.5 py-1 text-[11px] font-semibold text-[#93d75f] transition-colors hover:bg-[#6fbe44]/25"
              >
                {EmployeeRoleLabels[employee.role]}
              </button>

              {employee.requestedRole !== null && (
                <div className="flex flex-wrap items-center gap-1.5 rounded-lg bg-[#f5a83c]/10 px-2.5 py-1.5 text-[11px] text-[#f5a83c]">
                  <span>Requested: {EmployeeRoleLabels[employee.requestedRole]}</span>
                  <button
                    type="button"
                    onClick={() => handleApproveRequestedRole(employee)}
                    className="ml-auto rounded-full p-1 hover:bg-[#f5a83c]/20"
                    aria-label="Approve requested role"
                  >
                    <ShieldCheck className="h-3.5 w-3.5" />
                  </button>
                  <button
                    type="button"
                    onClick={() => handleDismissRequestedRole(employee)}
                    className="rounded-full p-1 hover:bg-[#f5a83c]/20"
                    aria-label="Dismiss requested role"
                  >
                    <ShieldX className="h-3.5 w-3.5" />
                  </button>
                </div>
              )}

              <div className="mt-auto flex items-center gap-1.5 pt-1">
                <button
                  type="button"
                  disabled={generatingId === employee.id}
                  onClick={() => handleDownloadPdf(employee)}
                  className="flex flex-1 items-center justify-center gap-1.5 rounded-lg border border-white/15 px-2.5 py-1.5 text-[11px] font-medium text-white/70 transition-colors hover:bg-white/5 disabled:opacity-50"
                >
                  <Download className="h-3.5 w-3.5" />
                  {generatingId === employee.id ? "Generating..." : "Performance PDF"}
                </button>
                {isHR && (
                  <button
                    type="button"
                    onClick={() => setStatusTarget(employee)}
                    className="flex items-center justify-center gap-1.5 rounded-lg border border-white/15 px-2.5 py-1.5 text-[11px] font-medium text-white/70 transition-colors hover:bg-white/5"
                  >
                    <UserCog className="h-3.5 w-3.5" />
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-3">
          <button
            type="button"
            disabled={page <= 1}
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            className="flex h-8 w-8 items-center justify-center rounded-lg border border-white/15 text-white/70 transition-colors hover:bg-white/5 disabled:opacity-30"
          >
            <ChevronLeft className="h-4 w-4" />
          </button>
          <span className="text-xs text-white/40">
            Page {page} of {totalPages}
          </span>
          <button
            type="button"
            disabled={page >= totalPages}
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            className="flex h-8 w-8 items-center justify-center rounded-lg border border-white/15 text-white/70 transition-colors hover:bg-white/5 disabled:opacity-30"
          >
            <ChevronRight className="h-4 w-4" />
          </button>
        </div>
      )}

      {roleTarget && (
        <div className="fixed inset-0 z-[90] flex items-center justify-center bg-slate-950/60 px-4">
          <div className="w-full max-w-sm rounded-2xl border border-white/10 bg-[#3a3d40] p-6 shadow-2xl font-['Inter']">
            <h3 className="mb-4 text-sm font-semibold text-white">Change role — {roleTarget.fullName}</h3>
            <select
              value={pendingRole}
              onChange={(e) => setPendingRole(Number(e.target.value) as EmployeeRole)}
              className="w-full rounded-lg border border-white/10 bg-[#202325] px-3 py-2 text-sm text-white focus:border-[#6fbe44] focus:outline-none"
            >
              {ASSIGNABLE_ROLES.map((role) => (
                <option key={role} value={role}>
                  {EmployeeRoleLabels[role]}
                </option>
              ))}
            </select>
            {roleError && <p className="mt-2 text-sm text-[#e69c9c]">{roleError}</p>}
            <div className="mt-4 flex items-center gap-2">
              <button
                type="button"
                disabled={isSavingRole || pendingRole === roleTarget.role}
                onClick={handleSaveRole}
                className="rounded-lg bg-[#6fbe44] px-4 py-2 text-sm font-semibold text-[#131415] transition-colors hover:bg-[#93d75f] disabled:opacity-50"
              >
                {isSavingRole ? "Saving..." : "Save"}
              </button>
              <button
                type="button"
                onClick={() => setRoleTarget(null)}
                className="text-xs text-white/40 hover:text-white/70 hover:underline"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {statusTarget && (
        <ConfirmDialog
          title={statusTarget.isActive ? "Deactivate colleague" : "Reactivate colleague"}
          message={
            statusTarget.isActive
              ? `Deactivate ${statusTarget.fullName}? They'll immediately lose access to the app.`
              : `Reactivate ${statusTarget.fullName}? They'll regain access to the app.`
          }
          confirmLabel={statusTarget.isActive ? "Deactivate" : "Reactivate"}
          isBusy={isSavingStatus}
          onConfirm={handleConfirmStatus}
          onCancel={() => setStatusTarget(null)}
        />
      )}
    </div>
  );
}
