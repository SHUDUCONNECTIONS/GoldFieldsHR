import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Mail } from "lucide-react";
import { getMyNotifications } from "../api/notifications";
import { formatDateTime } from "../lib/format";
import type { NotificationDto } from "../types/notification";

export function MailDropdown() {
  const navigate = useNavigate();
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  async function toggleOpen() {
    const next = !isOpen;
    setIsOpen(next);
    if (next) {
      try {
        setNotifications(await getMyNotifications());
      } catch {
        // Leave the previous list in place on failure.
      }
    }
  }

  function handleClick(notification: NotificationDto) {
    setIsOpen(false);
    if (notification.link) {
      navigate(notification.link);
    }
  }

  return (
    <div ref={containerRef} className="relative">
      <button
        type="button"
        onClick={toggleOpen}
        className="flex h-10 w-10 items-center justify-center rounded-md text-slate-500 hover:bg-slate-100 hover:text-slate-700"
        aria-label="Messages"
      >
        <Mail className="h-4 w-4" />
      </button>

      {isOpen && (
        <div className="absolute right-0 z-50 mt-2 w-80 max-w-[calc(100vw-1.5rem)] rounded-lg border border-slate-200 bg-white shadow-lg">
          <div className="border-b border-slate-100 px-4 py-2">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Messages</p>
          </div>
          <div className="max-h-96 overflow-y-auto">
            {notifications.length === 0 ? (
              <p className="px-4 py-8 text-center text-sm text-slate-500">No messages yet.</p>
            ) : (
              <ul className="divide-y divide-slate-100">
                {notifications.map((notification) => (
                  <li key={notification.id}>
                    <button
                      type="button"
                      onClick={() => handleClick(notification)}
                      className="flex w-full flex-col items-start gap-0.5 px-4 py-3 text-left text-sm text-slate-600 hover:bg-slate-50"
                    >
                      <span>{notification.message}</span>
                      <span className="text-xs font-normal text-slate-400">
                        {formatDateTime(notification.createdAtUtc)}
                      </span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
