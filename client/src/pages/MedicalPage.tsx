import { useCallback, useEffect, useRef, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { uploadAttachment } from "../api/attachments";
import { extractErrorMessage } from "../api/client";
import { getAllMedicalExaminations, getMyMedicalExaminations, recordMedicalExamination } from "../api/medical";
import { AttachmentsPanel } from "../components/AttachmentsPanel";
import { FitnessStatusBadge } from "../components/FitnessStatusBadge";
import { StepForm, type WizardStep } from "../components/StepForm";
import { formatDate } from "../lib/format";
import { AttachmentEntityType } from "../types/attachment";
import { EmployeeRole } from "../types/auth";
import { FitnessStatus, FitnessStatusLabels, type MedicalExaminationDto } from "../types/medical";
import { sanitizeEmployeeNumber } from "../utils/textInput";

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
  const [attachmentFile, setAttachmentFile] = useState<File | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

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

  async function handleSubmit() {
    setError(null);
    setIsSubmitting(true);
    try {
      const examination = await recordMedicalExamination({
        ...form,
        restrictions: form.restrictions || undefined,
        notes: form.notes || undefined,
      });
      if (attachmentFile) {
        try {
          await uploadAttachment(AttachmentEntityType.MedicalExamination, examination.id, attachmentFile);
        } catch (err) {
          setError(
            `The examination was recorded, but the attachment failed to upload: ${extractErrorMessage(err)}. Attach it below.`,
          );
          await loadAll();
          return;
        }
      }
      setForm(initialForm);
      setAttachmentFile(null);
      if (fileInputRef.current) fileInputRef.current.value = "";
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  const steps: WizardStep[] = [
    {
      title: "Employee & result",
      validate: () => {
        if (!form.employeeNumber) return "Please fill in all fields.";
        if (form.status === FitnessStatus.FitWithRestrictions && !form.restrictions) {
          return "Please describe the restrictions.";
        }
        return null;
      },
      content: (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Employee number
            <input
              required
              value={form.employeeNumber}
              onChange={(e) => setForm((prev) => ({ ...prev, employeeNumber: sanitizeEmployeeNumber(e.target.value) }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Fitness status
            <select
              value={form.status}
              onChange={(e) => setForm((prev) => ({ ...prev, status: Number(e.target.value) as FitnessStatus }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
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
          {form.status === FitnessStatus.FitWithRestrictions && (
            <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
              Restrictions
              <input
                required
                value={form.restrictions}
                onChange={(e) => setForm((prev) => ({ ...prev, restrictions: e.target.value }))}
                className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
              />
            </label>
          )}
        </div>
      ),
    },
    {
      title: "Dates & notes",
      validate: () => (!form.examDate || !form.expiryDate ? "Please fill in all fields." : null),
      content: (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Exam date
            <input
              type="date"
              required
              value={form.examDate}
              onChange={(e) => setForm((prev) => ({ ...prev, examDate: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Next exam due
            <input
              type="date"
              required
              value={form.expiryDate}
              onChange={(e) => setForm((prev) => ({ ...prev, expiryDate: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Notes (optional)
            <input
              value={form.notes}
              onChange={(e) => setForm((prev) => ({ ...prev, notes: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Attachment (optional)
            <input
              ref={fileInputRef}
              type="file"
              accept="application/pdf,image/jpeg,image/png"
              onChange={(e) => setAttachmentFile(e.target.files?.[0] ?? null)}
              className="rounded-md border border-slate-300 px-3 py-1.5 text-sm file:mr-2 file:rounded file:border-0 file:bg-slate-100 file:px-2 file:py-1 file:text-xs file:font-medium file:text-slate-700 focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>
        </div>
      ),
    },
  ];

  return (
    <div className="stagger-children flex flex-col gap-6">
      {isMedical && (
        <>
          <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
            <h3 className="mb-4 text-sm font-semibold text-slate-900">Record a medical examination</h3>
            <StepForm
              steps={steps}
              onSubmit={handleSubmit}
              submitLabel="Record examination"
              submittingLabel="Recording..."
              isSubmitting={isSubmitting}
              error={error}
            />
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
