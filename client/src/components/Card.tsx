import type { ReactNode } from "react";

interface CardProps {
  title?: string;
  action?: ReactNode;
  children: ReactNode;
  className?: string;
}

export function Card({ title, action, children, className }: CardProps) {
  return (
    <div className={`rounded-lg border border-slate-200 bg-white shadow-sm ${className ?? ""}`}>
      {title && (
        <div className="flex items-center justify-between rounded-t-lg border-b border-slate-200 bg-gradient-to-r from-[#f7f4ec] to-white px-6 py-4">
          <h3 className="text-sm font-semibold text-slate-900">{title}</h3>
          {action}
        </div>
      )}
      {children}
    </div>
  );
}
