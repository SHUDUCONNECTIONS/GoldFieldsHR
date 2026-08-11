import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessage } from "../api/client";
import { createAnnouncement, getAnnouncements } from "../api/announcements";
import { formatDateTime } from "../lib/format";
import { EmployeeRole } from "../types/auth";
import type { AnnouncementDto } from "../types/announcement";

export function AnnouncementsPage() {
  const { session } = useAuth();
  const canPublish = session?.role === EmployeeRole.HR || session?.role === EmployeeRole.Executive;

  const [announcements, setAnnouncements] = useState<AnnouncementDto[]>([]);
  const [title, setTitle] = useState("");
  const [body, setBody] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const loadAnnouncements = useCallback(async () => {
    try {
      const data = await getAnnouncements();
      setAnnouncements(data);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, []);

  useEffect(() => {
    loadAnnouncements();
  }, [loadAnnouncements]);

  async function handlePublish(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await createAnnouncement({ title, body });
      setTitle("");
      setBody("");
      await loadAnnouncements();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="stagger-children flex flex-col gap-6">
      {canPublish && (
        <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <h3 className="mb-4 text-sm font-semibold text-slate-900">Post an announcement</h3>
          <form onSubmit={handlePublish} className="flex flex-col gap-3">
            <label className="flex flex-col gap-1 text-sm text-slate-700">
              Title
              <input
                required
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm text-slate-700">
              Message
              <textarea
                required
                value={body}
                onChange={(e) => setBody(e.target.value)}
                rows={4}
                className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
              />
            </label>
            {error && <p className="text-sm text-red-600">{error}</p>}
            <button
              type="submit"
              disabled={isSubmitting}
              className="mt-1 w-fit rounded-md bg-slate-950 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-50"
            >
              {isSubmitting ? "Posting..." : "Post"}
            </button>
          </form>
        </div>
      )}

      <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 px-6 py-4">
          <h3 className="text-sm font-semibold text-slate-900">Announcements</h3>
        </div>
        {announcements.length === 0 ? (
          <p className="px-6 py-8 text-center text-sm text-slate-500">No announcements yet.</p>
        ) : (
          <ul className="divide-y divide-slate-100">
            {announcements.map((announcement) => (
              <li key={announcement.id} className="px-6 py-4">
                <p className="text-sm font-medium text-slate-900">{announcement.title}</p>
                <p className="text-xs text-slate-500">
                  Posted by {announcement.postedByName} — {formatDateTime(announcement.createdAtUtc)}
                </p>
                <p className="mt-2 whitespace-pre-wrap text-sm text-slate-600">{announcement.body}</p>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
