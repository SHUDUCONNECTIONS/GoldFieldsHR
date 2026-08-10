import { useCallback, useEffect, useState, type FormEvent } from "react";
import { extractErrorMessage } from "../api/client";
import { createSite, getAllSites, setSiteActiveStatus, updateSite } from "../api/sites";
import { Badge } from "./Badge";
import { ConfirmDialog } from "./ConfirmDialog";
import { useToast } from "./ToastProvider";
import type { SiteAdminDto } from "../types/site";

interface SiteManagementProps {
  canManage: boolean;
}

const initialForm = { name: "", location: "" };

export function SiteManagement({ canManage }: SiteManagementProps) {
  const { showSuccess, showError } = useToast();
  const [sites, setSites] = useState<SiteAdminDto[]>([]);
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editForm, setEditForm] = useState(initialForm);
  const [confirmDeactivate, setConfirmDeactivate] = useState<SiteAdminDto | null>(null);

  const load = useCallback(async () => {
    try {
      setSites(await getAllSites());
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  async function handleCreate(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await createSite(form);
      showSuccess(`Site "${form.name}" added.`);
      setForm(initialForm);
      await load();
    } catch (err) {
      const message = extractErrorMessage(err);
      setError(message);
      showError(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  function startEdit(site: SiteAdminDto) {
    setEditingId(site.id);
    setEditForm({ name: site.name, location: site.location });
  }

  async function confirmEdit(id: string) {
    setBusyId(id);
    setError(null);
    try {
      await updateSite(id, editForm);
      showSuccess("Site updated.");
      setEditingId(null);
      await load();
    } catch (err) {
      const message = extractErrorMessage(err);
      setError(message);
      showError(message);
    } finally {
      setBusyId(null);
    }
  }

  function handleToggleActiveClick(site: SiteAdminDto) {
    if (site.isActive) {
      setConfirmDeactivate(site);
    } else {
      void handleToggleActive(site);
    }
  }

  async function handleToggleActive(site: SiteAdminDto) {
    setBusyId(site.id);
    setError(null);
    try {
      await setSiteActiveStatus(site.id, { isActive: !site.isActive });
      showSuccess(`${site.name} was ${site.isActive ? "deactivated" : "reactivated"}.`);
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

  return (
    <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-200 px-6 py-4">
        <h3 className="text-sm font-semibold text-slate-900">Site management</h3>
      </div>

      {canManage && (
        <form onSubmit={handleCreate} className="flex flex-wrap items-end gap-3 border-b border-slate-100 px-6 py-4">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Name
            <input
              required
              value={form.name}
              onChange={(e) => setForm((prev) => ({ ...prev, name: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Location
            <input
              required
              value={form.location}
              onChange={(e) => setForm((prev) => ({ ...prev, location: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            />
          </label>
          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-md bg-slate-950 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-50"
          >
            {isSubmitting ? "Adding..." : "Add site"}
          </button>
        </form>
      )}

      {error && <p className="px-6 pt-4 text-sm text-red-600">{error}</p>}

      {sites.length === 0 ? (
        <p className="px-6 py-8 text-center text-sm text-slate-500">No sites on record.</p>
      ) : (
        <table className="w-full text-left text-sm">
          <thead>
            <tr className="text-xs uppercase tracking-wide text-slate-500">
              <th className="px-6 py-2 font-medium">Name</th>
              <th className="px-6 py-2 font-medium">Location</th>
              <th className="px-6 py-2 font-medium">Employees</th>
              <th className="px-6 py-2 font-medium">Status</th>
              {canManage && <th className="px-6 py-2 font-medium">Actions</th>}
            </tr>
          </thead>
          <tbody>
            {sites.map((site) => (
              <tr key={site.id} className="border-t border-slate-100">
                {editingId === site.id ? (
                  <>
                    <td className="px-6 py-2">
                      <input
                        value={editForm.name}
                        onChange={(e) => setEditForm((prev) => ({ ...prev, name: e.target.value }))}
                        className="w-32 rounded-md border border-slate-300 px-2 py-1 text-xs focus:border-amber-500 focus:outline-none"
                      />
                    </td>
                    <td className="px-6 py-2">
                      <input
                        value={editForm.location}
                        onChange={(e) => setEditForm((prev) => ({ ...prev, location: e.target.value }))}
                        className="w-32 rounded-md border border-slate-300 px-2 py-1 text-xs focus:border-amber-500 focus:outline-none"
                      />
                    </td>
                    <td className="px-6 py-2 text-slate-700">{site.employeeCount}</td>
                    <td className="px-6 py-2">
                      <Badge label={site.isActive ? "Active" : "Inactive"} tone={site.isActive ? "emerald" : "red"} />
                    </td>
                    <td className="px-6 py-2">
                      <div className="flex gap-2">
                        <button
                          type="button"
                          disabled={busyId === site.id}
                          onClick={() => confirmEdit(site.id)}
                          className="rounded-md bg-slate-950 px-3 py-1 text-xs font-medium text-white hover:bg-slate-800 disabled:opacity-50"
                        >
                          Save
                        </button>
                        <button
                          type="button"
                          onClick={() => setEditingId(null)}
                          className="text-xs text-slate-500 hover:underline"
                        >
                          Cancel
                        </button>
                      </div>
                    </td>
                  </>
                ) : (
                  <>
                    <td className="px-6 py-2 text-slate-700">{site.name}</td>
                    <td className="px-6 py-2 text-slate-700">{site.location}</td>
                    <td className="px-6 py-2 text-slate-700">{site.employeeCount}</td>
                    <td className="px-6 py-2">
                      <Badge label={site.isActive ? "Active" : "Inactive"} tone={site.isActive ? "emerald" : "red"} />
                    </td>
                    {canManage && (
                      <td className="px-6 py-2">
                        <div className="flex gap-2">
                          <button
                            type="button"
                            onClick={() => startEdit(site)}
                            className="rounded-md border border-slate-300 px-3 py-1 text-xs font-medium text-slate-700 hover:bg-slate-50"
                          >
                            Edit
                          </button>
                          <button
                            type="button"
                            disabled={busyId === site.id}
                            onClick={() => handleToggleActiveClick(site)}
                            className="rounded-md border border-slate-300 px-3 py-1 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-50"
                          >
                            {site.isActive ? "Deactivate" : "Reactivate"}
                          </button>
                        </div>
                      </td>
                    )}
                  </>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {confirmDeactivate && (
        <ConfirmDialog
          title="Deactivate site?"
          message={`"${confirmDeactivate.name}" will no longer appear as an option when registering new employees.`}
          confirmLabel="Deactivate"
          isBusy={busyId === confirmDeactivate.id}
          onConfirm={() => handleToggleActive(confirmDeactivate)}
          onCancel={() => setConfirmDeactivate(null)}
        />
      )}
    </div>
  );
}
