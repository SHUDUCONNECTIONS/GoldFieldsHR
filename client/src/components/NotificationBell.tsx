import { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Bell } from "lucide-react";
import {
  getMyNotifications,
  getUnreadNotificationCount,
  markAllNotificationsAsRead,
  markNotificationAsRead,
} from "../api/notifications";
import { formatDateTime } from "../lib/format";
import { playNotificationSound } from "../lib/notificationSound";
import type { NotificationDto } from "../types/notification";

const POLL_INTERVAL_MS = 30_000;

export function NotificationBell() {
  const navigate = useNavigate();
  const [unreadCount, setUnreadCount] = useState(0);
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const previousUnreadCountRef = useRef<number | null>(null);

  const refreshCount = useCallback(async () => {
    try {
      const count = await getUnreadNotificationCount();
      // Only alarm on a genuine increase since the last poll — not on first
      // load, where any already-unread notifications shouldn't trigger it.
      if (previousUnreadCountRef.current !== null && count > previousUnreadCountRef.current) {
        playNotificationSound();
      }
      previousUnreadCountRef.current = count;
      setUnreadCount(count);
    } catch {
      // Silently ignore — the bell just won't update this cycle.
    }
  }, []);

  useEffect(() => {
    refreshCount();
    const interval = setInterval(refreshCount, POLL_INTERVAL_MS);
    return () => clearInterval(interval);
  }, [refreshCount]);

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

  async function handleNotificationClick(notification: NotificationDto) {
    if (!notification.isRead) {
      try {
        await markNotificationAsRead(notification.id);
        setNotifications((prev) => prev.map((n) => (n.id === notification.id ? { ...n, isRead: true } : n)));
        setUnreadCount((prev) => Math.max(0, prev - 1));
      } catch {
        // Non-fatal — still navigate even if marking as read failed.
      }
    }
    setIsOpen(false);
    if (notification.link) {
      navigate(notification.link);
    }
  }

  async function handleMarkAllAsRead() {
    try {
      await markAllNotificationsAsRead();
      setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
      setUnreadCount(0);
    } catch {
      // Non-fatal
    }
  }

  return (
    <div ref={containerRef} className="relative">
      <button
        type="button"
        onClick={toggleOpen}
        className="relative flex h-10 w-10 items-center justify-center rounded-md text-slate-500 hover:bg-slate-100 hover:text-slate-700"
        aria-label="Notifications"
      >
        <Bell className="h-4 w-4" />
        {unreadCount > 0 && (
          <span className="notification-dot-pulse absolute -right-1 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-600 px-1 text-[10px] font-semibold text-white">
            {unreadCount > 99 ? "99+" : unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div className="absolute right-0 z-50 mt-2 w-80 max-w-[calc(100vw-1.5rem)] rounded-lg border border-slate-200 bg-white shadow-lg">
          <div className="flex items-center justify-between border-b border-slate-100 px-4 py-2">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Notifications</p>
            {unreadCount > 0 && (
              <button type="button" onClick={handleMarkAllAsRead} className="text-xs text-yellow-600 hover:underline">
                Mark all as read
              </button>
            )}
          </div>
          <div className="max-h-96 overflow-y-auto">
            {notifications.length === 0 ? (
              <p className="px-4 py-8 text-center text-sm text-slate-500">No notifications yet.</p>
            ) : (
              <ul className="divide-y divide-slate-100">
                {notifications.map((notification) => (
                  <li key={notification.id}>
                    <button
                      type="button"
                      onClick={() => handleNotificationClick(notification)}
                      className={`flex w-full flex-col items-start gap-0.5 px-4 py-3 text-left text-sm hover:bg-slate-50 ${
                        notification.isRead ? "text-slate-500" : "bg-yellow-50/50 font-medium text-slate-900"
                      }`}
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
