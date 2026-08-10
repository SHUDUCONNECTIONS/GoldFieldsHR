import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessage } from "../api/client";
import { getAllMedicalExaminations, getMyMedicalExaminations, recordMedicalExamination } from "../api/medical";
import { AttachmentsPanel } from "../components/AttachmentsPanel";
import { FitnessStatusBadge } from "../components/FitnessStatusBadge";
import { formatDate } from "../lib/format";
import { AttachmentEntityType } from "../types/attachment";
import { EmployeeRole } from "../types/auth";
import { FitnessStatus, FitnessStatusLabels, type MedicalExaminationDto } from "../types/medical";

const initialForm = {
  employeeNumber: "",
  examDate: "",
  expiryDate: "",
  status: FitnessStatus.Fit as FitnessStatus,
  restrictions: "",
  notes: "",
};

export function MedicalPage() {
  const { session } = useAuth();
  const isMedical = session?.role === EmployeeRole.Medical;

  const [myExaminations, setMyExaminations] = useState<MedicalExaminationDto[]>([]);
  const [allExaminations, setAllExaminations] = useState<MedicalExaminationDto[]>([]);
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const loadAll = useCallback(async () => {
    try {
      const requests: Promise<unknown>[] = [getMyMedicalExaminations().then(setMyExaminations)];
      if (isMedical) requests.push(getAllMedicalExaminations().then(setAllExaminations));
      await Promise.all(requests);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, [isMedical]);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await recordMedicalExamination({
        ...form,
        restrictions: form.restrictions || undefined,
        notes: form.notes || undefined,
      });
      setForm(initialForm);
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="flex flex-col gap-6">
      {isMedical && (
        <>
          <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
            <h3 className="mb-4 text-sm font-semibold text-slate-900">Record a medical examination</h3>
            <form onSubmit={handleSubmit} className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <label className="flex flex-col gap-1 text-sm text-slate-700">
                Employee number
                <input
                  required
                  value={form.employeeNumber}
                  onChange={(e) => setForm((prev) => ({ ...prev, employeeNumber: e.target.value }))}
                  className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
                />
              </label>
              <label className="flex flex-col gap-1 text-sm text-slate-700">
                Fitness status
                <select
                  value={form.status}
                  onChange={(e) => setForm((prev) => ({ ...prev, status: Number(e.target.value) as FitnessStatus }))}
                  className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
                >
                  {Object.values(FitnessStatus)
                    .filter((v): v is FitnessStatus => typeof v === "number")
                    .map((value) => (
                      <option key={value} value={value}>
                        {FitnessStatusLabels[value]}
                      </option>
                    ))}
                </select>
              </label>
              <label className="flex flex-col gap-1 text-sm text-slate-700">
                Exam date
                <input
                  type="date"
                  required
                  value={form.examDate}
                  onChange={(e) => setForm((prev) => ({ ...prev, examDate: e.target.value }))}
                  className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
                />
              </label>
              <label className="flex flex-col gap-1 text-sm text-slate-700">
                Next exam due
                <input
                  type="date"
                  required
                  value={form.expiryDate}
                  onChange={(e) => setForm((prev) => ({ ...prev, expiryDate: e.target.value }))}
                  className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
                />
              </label>
              {form.status === FitnessStatus.FitWithRestrictions && (
                <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
                  Restrictions
                  <input
                    required
                    value={form.restrictions}
                    onChange={(e) => setForm((prev) => ({ ...prev, restrictions: e.target.value }))}
                    className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
                  />
                </label>
              )}
              <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
                Notes (optional)
                <input
                  value={form.notes}
                  onChange={(e) => setForm((prev) => ({ ...prev, notes: e.target.value }))}
                  className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
                />
              </label>

              {error && <p className="text-sm text-red-600 sm:col-span-2">{error}</p>}

              <button
                type="submit"
                disabled={isSubmitting}
                className="mt-1 w-fit rounded-md bg-slate-950 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-50 sm:col-span-2"
              >
                {isSubmitting ? "Recording..." : "Record examination"}
              </button>
            </form>
          </div>

          <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
            <div className="border-b border-slate-200 px-6 py-4">
              <h3 className="text-sm font-semibold text-slate-900">All examinations</h3>
            </div>
            {allExaminations.length === 0 ? (
              <p className="px-6 py-8 text-center text-sm text-slate-500">No examinations recorded yet.</p>
            ) : (
              <table className="w-full text-left text-sm">
                <thead>
                  <tr className="text-xs uppercase tracking-wide text-slate-500">
                    <th className="px-6 py-2 font-medium">Employee</th>
                    <th className="px-6 py-2 font-medium">Exam date</th>
                    <th className="px-6 py-2 font-medium">Next due</th>
                    <th className="px-6 py-2 font-medium">Status</th>
                    <th className="px-6 py-2 font-medium">Examined by</th>
                    <th className="px-6 py-2 font-medium">Attachments</th>
                  </tr>
                </thead>
                <tbody>
                  {allExaminations.map((exam) => (
                    <tr key={exam.id} className="border-t border-slate-100">
                      <td className="px-6 py-2 text-slate-700">{exam.employeeName}</td>
                      <td className="px-6 py-2 text-slate-700">{formatDate(exam.examDate)}</td>
                      <td className="px-6 py-2 text-slate-700">{formatDate(exam.expiryDate)}</td>
                      <td className="px-6 py-2">
                        <FitnessStatusBadge status={exam.status} />
                      </td>
                      <td className="px-6 py-2 text-slate-500">{exam.examinedByName}</td>
                      <td className="px-6 py-2">
                        <AttachmentsPanel
                          entityType={AttachmentEntityType.MedicalExamination}
                          entityId={exam.id}
                          canUpload
                          compact
                        />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </>
      )}

      <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 px-6 py-4">
          <h3 className="text-sm font-semibold text-slate-900">My medical record</h3>
        </div>
        {myExaminations.length === 0 ? (
          <p className="px-6 py-8 text-center text-sm text-slate-500">No examinations on record yet.</p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="text-xs uppercase tracking-wide text-slate-500">
                <th className="px-6 py-2 font-medium">Exam date</th>
                <th className="px-6 py-2 font-medium">Next due</th>
                <th className="px-6 py-2 font-medium">Status</th>
                <th className="px-6 py-2 font-medium">Restrictions</th>
                <th className="px-6 py-2 font-medium">Examined by</th>
                <th className="px-6 py-2 font-medium">Attachments</th>
              </tr>
            </thead>
            <tbody>
              {myExaminations.map((exam) => (
                <tr key={exam.id} className="border-t border-slate-100">
                  <td className="px-6 py-2 text-slate-700">{formatDate(exam.examDate)}</td>
                  <td className="px-6 py-2 text-slate-700">{formatDate(exam.expiryDate)}</td>
                  <td className="px-6 py-2">
                    <FitnessStatusBadge status={exam.status} />
                  </td>
                  <td className="px-6 py-2 text-slate-500">{exam.restrictions ?? "—"}</td>
                  <td className="px-6 py-2 text-slate-500">{exam.examinedByName}</td>
                  <td className="px-6 py-2">
                    <AttachmentsPanel
                      entityType={AttachmentEntityType.MedicalExamination}
                      entityId={exam.id}
                      canUpload={isMedical}
                      compact
                    />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
