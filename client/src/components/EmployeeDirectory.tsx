import { useCallback, useEffect, useState } from "react";
import { Download } from "lucide-react";
import { extractErrorMessage } from "../api/client";
import {
  dismissRequestedRole,
  getEmployees,
  setEmployeeActiveStatus,
  setEmployeeManager,
  setEmployeeRole,
} from "../api/employees";
import { downloadCsv } from "../lib/csv";
import { Badge } from "./Badge";
import { ConfirmDialog } from "./ConfirmDialog";
import { RoleRequestQueue } from "./RoleRequestQueue";
import { useToast } from "./ToastProvider";
import { EmployeeRole, EmployeeRoleLabels } from "../types/auth";
import type { EmployeeSummaryDto } from "../types/employee";
import { sanitizeEmployeeNumber } from "../utils/textInput";

interface EmployeeDirectoryProps {
  canManage: boolean;
  /** Grant/dismiss pending role requests — HR has this via canManage; Executive gets it without full admin rights. */
  canApproveRoleRequests?: boolean;
}

const PAGE_SIZE = 25;

export function EmployeeDirectory({ canManage, canApproveRoleRequests = canManage }: EmployeeDirectoryProps) {
  const { showSuccess, showError } = useToast();
  const [employees, setEmployees] = useState<EmployeeSummaryDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [editingManagerId, setEditingManagerId] = useState<string | null>(null);
  const [managerNumberInput, setManagerNumberInput] = useState("");
  const [editingRoleId, setEditingRoleId] = useState<string | null>(null);
  const [roleInput, setRoleInput] = useState<EmployeeRole>(EmployeeRole.Employee);
  const [searchInput, setSearchInput] = useState("");
  const [searchQuery, setSearchQuery] = useState("");
  const [roleFilter, setRoleFilter] = useState<EmployeeRole | "all">("all");
  const [statusFilter, setStatusFilter] = useState<"all" | "active" | "inactive">("all");
  const [confirmDeactivate, setConfirmDeactivate] = useState<EmployeeSummaryDto | null>(null);
  const [pendingRequests, setPendingRequests] = useState<EmployeeSummaryDto[]>([]);

  // Debounce free-text search so we don't fire a request on every keystroke.
  useEffect(() => {
    const timeout = setTimeout(() => setSearchQuery(searchInput.trim()), 300);
    return () => clearTimeout(timeout);
  }, [searchInput]);

  // Any filter change resets back to page 1.
  useEffect(() => {
    setPage(1);
  }, [searchQuery, roleFilter, statusFilter]);

  const load = useCallback(async () => {
    try {
      const data = await getEmployees({
        search: searchQuery || undefined,
        role: roleFilter === "all" ? undefined : roleFilter,
        isActive: statusFilter === "all" ? undefined : statusFilter === "active",
        page,
        pageSize: PAGE_SIZE,
      });
      setEmployees(data.items);
      setTotalCount(data.totalCount);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, [searchQuery, roleFilter, statusFilter, page]);

  useEffect(() => {
    load();
  }, [load]);

  const loadPendingRequests = useCallback(async () => {
    if (!canApproveRoleRequests) return;
    try {
      const data = await getEmployees({ hasRequestedRole: true, pageSize: 100 });
      setPendingRequests(data.items);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, [canApproveRoleRequests]);

  useEffect(() => {
    loadPendingRequests();
  }, [loadPendingRequests]);

  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  function handleToggleActiveClick(employee: EmployeeSummaryDto) {
    if (employee.isActive) {
      setConfirmDeactivate(employee);
    } else {
      void handleToggleActive(employee);
    }
  }

  async function handleToggleActive(employee: EmployeeSummaryDto) {
    setBusyId(employee.id);
    setError(null);
    try {
      await setEmployeeActiveStatus(employee.id, { isActive: !employee.isActive });
      showSuccess(`${employee.fullName} was ${employee.isActive ? "deactivated" : "reactivated"}.`);
      await load();
    } catch (err) {
      const message = extractErrorMessage(err);
      setError(message);
      showError(message);
    } finally {
      setBusyId(null);
      setConfirmDeactivate(null);
    }
  }

  function startEditManager(employee: EmployeeSummaryDto) {
    setEditingManagerId(employee.id);
    setManagerNumberInput(employee.managerId ? "" : "");
  }

  async function confirmEditManager(employeeId: string) {
    setBusyId(employeeId);
    setError(null);
    try {
      await setEmployeeManager(employeeId, { managerEmployeeNumber: managerNumberInput || undefined });
      showSuccess("Manager updated.");
      setEditingManagerId(null);
      setManagerNumberInput("");
      await load();
    } catch (err) {
      const message = extractErrorMessage(err);
      setError(message);
      showError(message);
    } finally {
      setBusyId(null);
    }
  }

  function startEditRole(employee: EmployeeSummaryDto) {
    setEditingRoleId(employee.id);
    setRoleInput(employee.role);
  }

  async function handleExportCsv() {
    setError(null);
    try {
      // Export every row matching the current filters, not just the page on screen.
      const data = await getEmployees({
        search: searchQuery || undefined,
        role: roleFilter === "all" ? undefined : roleFilter,
        isActive: statusFilter === "all" ? undefined : statusFilter === "active",
        page: 1,
        pageSize: Math.max(totalCount, 1),
      });
      downloadCsv(
        `employee-directory-${new Date().toISOString().slice(0, 10)}.csv`,
        ["Name", "Employee #", "Email", "Job title", "Role", "Site", "Manager", "Status"],
        data.items.map((e) => [
          e.fullName,
          e.employeeNumber,
          e.email,
          e.jobTitle,
          EmployeeRoleLabels[e.role],
          e.siteName,
          e.managerName ?? "",
          e.isActive ? "Active" : "Inactive",
        ]),
      );
      showSuccess(`Exported ${data.items.length} employee${data.items.length === 1 ? "" : "s"} to CSV.`);
    } catch (err) {
      const message = extractErrorMessage(err);
      setError(message);
      showError(message);
    }
  }

  async function confirmEditRole(employeeId: string) {
    setBusyId(employeeId);
    setError(null);
    try {
      await setEmployeeRole(employeeId, { role: roleInput });
      showSuccess("Role updated.");
      setEditingRoleId(null);
      await load();
    } catch (err) {
      const message = extractErrorMessage(err);
      setError(message);
      showError(message);
    } finally {
      setBusyId(null);
    }
  }

  async function handleGrantRequestedRole(employee: EmployeeSummaryDto) {
    if (employee.requestedRole === null) return;
    setBusyId(employee.id);
    setError(null);
    try {
      await setEmployeeRole(employee.id, { role: employee.requestedRole });
      showSuccess(`${employee.fullName} was granted the ${EmployeeRoleLabels[employee.requestedRole]} role.`);
      await Promise.all([load(), loadPendingRequests()]);
    } catch (err) {
      const message = extractErrorMessage(err);
      setError(message);
      showError(message);
    } finally {
      setBusyId(null);
    }
  }

  async function handleDismissRequestedRole(employee: EmployeeSummaryDto) {
    setBusyId(employee.id);
    setError(null);
    try {
      await dismissRequestedRole(employee.id);
      showSuccess(`Dismissed ${employee.fullName}'s role request.`);
      await Promise.all([load(), loadPendingRequests()]);
    } catch (err) {
      const message = extractErrorMessage(err);
      setError(message);
      showError(message);
    } finally {
      setBusyId(null);
    }
  }

  return (
    <div className="flex flex-col gap-6">
      {canApproveRoleRequests && (
        <RoleRequestQueue
          items={pendingRequests}
          isBusy={busyId !== null}
          onGrant={handleGrantRequestedRole}
          onDismiss={handleDismissRequestedRole}
        />
      )}

      <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200 px-6 py-4">
        <h3 className="text-sm font-semibold text-slate-900">Employee directory</h3>
        <div className="flex flex-wrap items-center gap-2">
          <input
            type="text"
            placeholder="Search name, #, email, title..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="w-56 rounded-md border border-slate-300 px-2 py-1.5 text-xs focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
          />
          <select
            value={roleFilter}
            onChange={(e) => setRoleFilter(e.target.value === "all" ? "all" : (Number(e.target.value) as EmployeeRole))}
            className="rounded-md border border-slate-300 px-2 py-1.5 text-xs focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
          >
            <option value="all">All roles</option>
            {Object.entries(EmployeeRoleLabels).map(([value, label]) => (
              <option key={value} value={value}>
                {label}
              </option>
            ))}
          </select>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as "all" | "active" | "inactive")}
            className="rounded-md border border-slate-300 px-2 py-1.5 text-xs focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
          >
            <option value="all">All statuses</option>
            <option value="active">Active</option>
            <option value="inactive">Inactive</option>
          </select>
          <button
            type="button"
            onClick={handleExportCsv}
            className="flex items-center gap-1.5 rounded-md border border-slate-300 px-3 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-50"
          >
            <Download className="h-3.5 w-3.5" />
            Export CSV
          </button>
        </div>
      </div>

      {error && <p className="px-6 pt-4 text-sm text-red-600">{error}</p>}

      {employees.length === 0 ? (
        <p className="px-6 py-8 text-center text-sm text-slate-500">
          {totalCount === 0 ? "No employees on record." : "No employees match your filters."}
        </p>
      ) : (
        <table className="w-full text-left text-sm">
          <thead>
            <tr className="text-xs uppercase tracking-wide text-slate-500">
              <th className="px-6 py-2 font-medium">Name</th>
              <th className="px-6 py-2 font-medium">Employee #</th>
              <th className="px-6 py-2 font-medium">Email</th>
              <th className="px-6 py-2 font-medium">Job title</th>
              <th className="px-6 py-2 font-medium">Role</th>
              <th className="px-6 py-2 font-medium">Site</th>
              <th className="px-6 py-2 font-medium">Manager</th>
              <th className="px-6 py-2 font-medium">Status</th>
              {canManage && <th className="px-6 py-2 font-medium">Actions</th>}
            </tr>
          </thead>
          <tbody>
            {employees.map((employee) => (
              <tr key={employee.id} className="border-t border-slate-100">
                <td className="px-6 py-2 text-slate-700">{employee.fullName}</td>
                <td className="px-6 py-2 text-slate-700">{employee.employeeNumber}</td>
                <td className="px-6 py-2 text-slate-500">{employee.email}</td>
                <td className="px-6 py-2 text-slate-700">{employee.jobTitle}</td>
                <td className="px-6 py-2 text-slate-700">
                  {editingRoleId === employee.id ? (
                    <div className="flex items-center gap-1">
                      <select
                        value={roleInput}
                        onChange={(e) => setRoleInput(Number(e.target.value) as EmployeeRole)}
                        className="rounded-md border border-slate-300 px-2 py-1 text-xs focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
                      >
                        {Object.entries(EmployeeRoleLabels).map(([value, label]) => (
                          <option key={value} value={value}>
                            {label}
                          </option>
                        ))}
                      </select>
                      <button
                        type="button"
                        disabled={busyId === employee.id}
                        onClick={() => confirmEditRole(employee.id)}
                        className="rounded-md bg-red-600 px-2 py-1 text-xs font-medium text-white hover:bg-red-500 disabled:opacity-50"
                      >
                        Save
                      </button>
                      <button
                        type="button"
                        onClick={() => setEditingRoleId(null)}
                        className="text-xs text-slate-500 hover:underline"
                      >
                        Cancel
                      </button>
                    </div>
                  ) : (
                    EmployeeRoleLabels[employee.role]
                  )}
                </td>
                <td className="px-6 py-2 text-slate-700">{employee.siteName}</td>
                <td className="px-6 py-2 text-slate-700">
                  {editingManagerId === employee.id ? (
                    <div className="flex items-center gap-1">
                      <input
                        type="text"
                        placeholder="Employee #"
                        value={managerNumberInput}
                        onChange={(e) => setManagerNumberInput(sanitizeEmployeeNumber(e.target.value))}
                        className="w-28 rounded-md border border-slate-300 px-2 py-1 text-xs focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
                      />
                      <button
                        type="button"
                        disabled={busyId === employee.id}
                        onClick={() => confirmEditManager(employee.id)}
                        className="rounded-md bg-red-600 px-2 py-1 text-xs font-medium text-white hover:bg-red-500 disabled:opacity-50"
                      >
                        Save
                      </button>
                      <button
                        type="button"
                        onClick={() => setEditingManagerId(null)}
                        className="text-xs text-slate-500 hover:underline"
                      >
                        Cancel
                      </button>
                    </div>
                  ) : (
                    employee.managerName ?? "—"
                  )}
                </td>
                <td className="px-6 py-2">
                  <Badge label={employee.isActive ? "Active" : "Inactive"} tone={employee.isActive ? "emerald" : "red"} />
                </td>
                {canManage && (
                  <td className="px-6 py-2">
                    <div className="flex gap-2">
                      <button
                        type="button"
                        disabled={busyId === employee.id}
                        onClick={() => handleToggleActiveClick(employee)}
                        className="rounded-md border border-slate-300 px-3 py-1 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-50"
                      >
                        {employee.isActive ? "Deactivate" : "Reactivate"}
                      </button>
                      {editingManagerId !== employee.id && (
                        <button
                          type="button"
                          onClick={() => startEditManager(employee)}
                          className="rounded-md border border-slate-300 px-3 py-1 text-xs font-medium text-slate-700 hover:bg-slate-50"
                        >
                          Set manager
                        </button>
                      )}
                      {editingRoleId !== employee.id && (
                        <button
                          type="button"
                          onClick={() => startEditRole(employee)}
                          className="rounded-md border border-slate-300 px-3 py-1 text-xs font-medium text-slate-700 hover:bg-slate-50"
                        >
                          Set role
                        </button>
                      )}
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {totalCount > 0 && (
        <div className="flex items-center justify-between border-t border-slate-200 px-6 py-3 text-xs text-slate-500">
          <span>
            Showing {(page - 1) * PAGE_SIZE + 1}–{Math.min(page * PAGE_SIZE, totalCount)} of {totalCount}
          </span>
          <div className="flex items-center gap-2">
            <button
              type="button"
              disabled={page <= 1}
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              className="rounded-md border border-slate-300 px-3 py-1 font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Previous
            </button>
            <span>
              Page {page} of {totalPages}
            </span>
            <button
              type="button"
              disabled={page >= totalPages}
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              className="rounded-md border border-slate-300 px-3 py-1 font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Next
            </button>
          </div>
        </div>
      )}

      {confirmDeactivate && (
        <ConfirmDialog
          title="Deactivate employee?"
          message={`${confirmDeactivate.fullName} will lose access to the portal. You can reactivate them at any time.`}
          confirmLabel="Deactivate"
          isBusy={busyId === confirmDeactivate.id}
          onConfirm={() => handleToggleActive(confirmDeactivate)}
          onCancel={() => setConfirmDeactivate(null)}
        />
      )}
      </div>
    </div>
  );
}
