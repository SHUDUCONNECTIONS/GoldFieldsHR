import { useCallback, useEffect, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessage } from "../api/client";
import {
  createPerformanceReview,
  getAllPerformanceReviews,
  getMyPerformanceReviews,
  getPerformanceReviewsGivenByMe,
} from "../api/performance";
import { ScoreBadge } from "../components/ScoreBadge";
import { StepForm, type WizardStep } from "../components/StepForm";
import { formatDateTime } from "../lib/format";
import { EmployeeRole } from "../types/auth";
import type { PerformanceReviewDto } from "../types/performance";
import { sanitizeEmployeeNumber } from "../utils/textInput";

const initialForm = {
  employeeNumber: "",
  periodLabel: "",
  score: 5,
  comments: "",
};

function average(reviews: PerformanceReviewDto[]): string {
  if (reviews.length === 0) return "—";
  return (reviews.reduce((sum, r) => sum + r.score, 0) / reviews.length).toFixed(1);
}

export function PerformanceReviewsPage() {
  const { session } = useAuth();
  const isLineManager = session?.role === EmployeeRole.LineManager;
  const isHR = session?.role === EmployeeRole.HR;

  const [myReviews, setMyReviews] = useState<PerformanceReviewDto[]>([]);
  const [givenReviews, setGivenReviews] = useState<PerformanceReviewDto[]>([]);
  const [allReviews, setAllReviews] = useState<PerformanceReviewDto[]>([]);
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const loadAll = useCallback(async () => {
    try {
      const requests: Promise<unknown>[] = [getMyPerformanceReviews().then(setMyReviews)];
      if (isLineManager) requests.push(getPerformanceReviewsGivenByMe().then(setGivenReviews));
      if (isHR) requests.push(getAllPerformanceReviews().then(setAllReviews));
      await Promise.all(requests);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, [isLineManager, isHR]);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  async function handleSubmit() {
    setError(null);
    setIsSubmitting(true);
    try {
      await createPerformanceReview({ ...form, comments: form.comments || undefined });
      setForm(initialForm);
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  const steps: WizardStep[] = [
    {
      title: "Who & when",
      validate: () => (!form.employeeNumber || !form.periodLabel ? "Please fill in all fields." : null),
      content: (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Employee number
            <input
              required
              value={form.employeeNumber}
              onChange={(e) => setForm((prev) => ({ ...prev, employeeNumber: sanitizeEmployeeNumber(e.target.value) }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Period (e.g. Q3 2026)
            <input
              required
              value={form.periodLabel}
              onChange={(e) => setForm((prev) => ({ ...prev, periodLabel: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            />
          </label>
        </div>
      ),
    },
    {
      title: "Score & comments",
      content: (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Score
            <select
              value={form.score}
              onChange={(e) => setForm((prev) => ({ ...prev, score: Number(e.target.value) }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            >
              {[5, 4, 3, 2, 1].map((value) => (
                <option key={value} value={value}>
                  {value} / 5
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Comments (optional)
            <textarea
              value={form.comments}
              onChange={(e) => setForm((prev) => ({ ...prev, comments: e.target.value }))}
              rows={2}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            />
          </label>
        </div>
      ),
    },
  ];

  return (
    <div className="stagger-children flex flex-col gap-6">
      {isLineManager && (
        <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <h3 className="mb-4 text-sm font-semibold text-slate-900">Give a performance review</h3>
          <StepForm
            steps={steps}
            onSubmit={handleSubmit}
            submitLabel="Submit review"
            submittingLabel="Submitting..."
            isSubmitting={isSubmitting}
            error={error}
          />
        </div>
      )}

      {isLineManager && (
        <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
          <div className="border-b border-slate-200 px-6 py-4">
            <h3 className="text-sm font-semibold text-slate-900">Reviews I've given</h3>
          </div>
          {givenReviews.length === 0 ? (
            <p className="px-6 py-8 text-center text-sm text-slate-500">You haven't given any reviews yet.</p>
          ) : (
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="text-xs uppercase tracking-wide text-slate-500">
                  <th className="px-6 py-2 font-medium">Employee</th>
                  <th className="px-6 py-2 font-medium">Period</th>
                  <th className="px-6 py-2 font-medium">Score</th>
                  <th className="px-6 py-2 font-medium">Given</th>
                </tr>
              </thead>
              <tbody>
                {givenReviews.map((r) => (
                  <tr key={r.id} className="border-t border-slate-100">
                    <td className="px-6 py-2 text-slate-700">{r.employeeName}</td>
                    <td className="px-6 py-2 text-slate-700">{r.periodLabel}</td>
                    <td className="px-6 py-2">
                      <ScoreBadge score={r.score} />
                    </td>
                    <td className="px-6 py-2 text-slate-700">{formatDateTime(r.createdAtUtc)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {isHR && (
        <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
          <div className="border-b border-slate-200 px-6 py-4">
            <h3 className="text-sm font-semibold text-slate-900">All reviews</h3>
          </div>
          {allReviews.length === 0 ? (
            <p className="px-6 py-8 text-center text-sm text-slate-500">No reviews recorded yet.</p>
          ) : (
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="text-xs uppercase tracking-wide text-slate-500">
                  <th className="px-6 py-2 font-medium">Employee</th>
                  <th className="px-6 py-2 font-medium">Reviewer</th>
                  <th className="px-6 py-2 font-medium">Period</th>
                  <th className="px-6 py-2 font-medium">Score</th>
                </tr>
              </thead>
              <tbody>
                {allReviews.map((r) => (
                  <tr key={r.id} className="border-t border-slate-100">
                    <td className="px-6 py-2 text-slate-700">{r.employeeName}</td>
                    <td className="px-6 py-2 text-slate-700">{r.reviewerName}</td>
                    <td className="px-6 py-2 text-slate-700">{r.periodLabel}</td>
                    <td className="px-6 py-2">
                      <ScoreBadge score={r.score} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
        <div className="flex items-center justify-between border-b border-slate-200 px-6 py-4">
          <h3 className="text-sm font-semibold text-slate-900">My reviews</h3>
          <span className="text-xs text-slate-500">Average: {average(myReviews)} / 5</span>
        </div>
        {myReviews.length === 0 ? (
          <p className="px-6 py-8 text-center text-sm text-slate-500">No reviews yet.</p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="text-xs uppercase tracking-wide text-slate-500">
                <th className="px-6 py-2 font-medium">Period</th>
                <th className="px-6 py-2 font-medium">Score</th>
                <th className="px-6 py-2 font-medium">Reviewer</th>
                <th className="px-6 py-2 font-medium">Comments</th>
              </tr>
            </thead>
            <tbody>
              {myReviews.map((r) => (
                <tr key={r.id} className="border-t border-slate-100">
                  <td className="px-6 py-2 text-slate-700">{r.periodLabel}</td>
                  <td className="px-6 py-2">
                    <ScoreBadge score={r.score} />
                  </td>
                  <td className="px-6 py-2 text-slate-700">{r.reviewerName}</td>
                  <td className="px-6 py-2 text-slate-500">{r.comments ?? "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
