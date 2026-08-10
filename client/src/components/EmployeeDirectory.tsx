import { useCallback, useEffect, useMemo, useState } from "react";
import { Download } from "lucide-react";
import { extractErrorMessage } from "../api/client";
import { getAllEmployees, setEmployeeActiveStatus, setEmployeeManager, setEmployeeRole } from "../api/employees";
import { downloadCsv } from "../lib/csv";
import { Badge } from "./Badge";
import { ConfirmDialog } from "./ConfirmDialog";
import { useToast } from "./ToastProvider";
import { EmployeeRole, EmployeeRoleLabels } from "../types/auth";
import type { EmployeeSummaryDto } from "../types/employee";

interface EmployeeDirectoryProps {
  canManage: boolean;
}

export function EmployeeDirectory({ canManage }: EmployeeDirectoryProps) {
  const { showSuccess, showError } = useToast();
  const [employees, setEmployees] = useState<EmployeeSummaryDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [editingManagerId, setEditingManagerId] = useState<string | null>(null);
  const [managerNumberInput, setManagerNumberInput] = useState("");
  const [editingRoleId, setEditingRoleId] = useState<string | null>(null);
  const [roleInput, setRoleInput] = useState<EmployeeRole>(EmployeeRole.Employee);
  const [searchQuery, setSearchQuery] = useState("");
  const [roleFilter, setRoleFilter] = useState<EmployeeRole | "all">("all");
  const [statusFilter, setStatusFilter] = useState<"all" | "active" | "inactive">("all");
  const [confirmDeactivate, setConfirmDeactivate] = useState<EmployeeSummaryDto | null>(null);

  const filteredEmployees = useMemo(() => {
    const query = searchQuery.trim().toLowerCase();
    return employees.filter((e) => {
      if (roleFilter !== "all" && e.role !== roleFilter) return false;
      if (statusFilter === "active" && !e.isActive) return false;
      if (statusFilter === "inactive" && e.isActive) return false;
      if (!query) return true;
      return (
        e.fullName.toLowerCase().includes(query) ||
        e.employeeNumber.toLowerCase().includes(query) ||
        e.email.toLowerCase().includes(query) ||
        e.jobTitle.toLowerCase().includes(query)
      );
    });
  }, [employees, searchQuery, roleFilter, statusFilter]);

  const load = useCallback(async () => {
    try {
      const data = await getAllEmployees();
      setEmployees(data);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

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

  function handleExportCsv() {
    downloadCsv(
      `employee-directory-${new Date().toISOString().slice(0, 10)}.csv`,
      ["Name", "Employee #", "Email", "Job title", "Role", "Site", "Manager", "Status"],
      filteredEmployees.map((e) => [
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
    showSuccess(`Exported ${filteredEmployees.length} employee${filteredEmployees.length === 1 ? "" : "s"} to CSV.`);
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

  return (
    <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200 px-6 py-4">
        <h3 className="text-sm font-semibold text-slate-900">Employee directory</h3>
        <div className="flex flex-wrap items-center gap-2">
          <input
            type="text"
            placeholder="Search name, #, email, title..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-56 rounded-md border border-slate-300 px-2 py-1.5 text-xs focus:border-amber-500 focus:outline-none"
          />
          <select
            value={roleFilter}
            onChange={(e) => setRoleFilter(e.target.value === "all" ? "all" : (Number(e.target.value) as EmployeeRole))}
            className="rounded-md border border-slate-300 px-2 py-1.5 text-xs focus:border-amber-500 focus:outline-none"
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
            className="rounded-md border border-slate-300 px-2 py-1.5 text-xs focus:border-amber-500 focus:outline-none"
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

      {filteredEmployees.length === 0 ? (
        <p className="px-6 py-8 text-center text-sm text-slate-500">
          {employees.length === 0 ? "No employees on record." : "No employees match your filters."}
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
            {filteredEmployees.map((employee) => (
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
                        className="rounded-md border border-slate-300 px-2 py-1 text-xs focus:border-amber-500 focus:outline-none"
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
                        className="rounded-md bg-slate-950 px-2 py-1 text-xs font-medium text-white hover:bg-slate-800 disabled:opacity-50"
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
                        onChange={(e) => setManagerNumberInput(e.target.value)}
                        className="w-28 rounded-md border border-slate-300 px-2 py-1 text-xs focus:border-amber-500 focus:outline-none"
                      />
                      <button
                        type="button"
                        disabled={busyId === employee.id}
                        onClick={() => confirmEditManager(employee.id)}
                        className="rounded-md bg-slate-950 px-2 py-1 text-xs font-medium text-white hover:bg-slate-800 disabled:opacity-50"
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
  );
}
