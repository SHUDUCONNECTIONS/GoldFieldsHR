import {
  LayoutDashboard,
  Clock,
  CalendarClock,
  CalendarOff,
  ShieldCheck,
  AlertTriangle,
  FileText,
  Megaphone,
  HeartPulse,
  GraduationCap,
  HardHat,
  Scale,
  TrendingUp,
  Award,
  BarChart3,
  Siren,
  Settings,
  type LucideIcon,
} from "lucide-react";
import { EmployeeRole } from "../types/auth";

export interface NavItem {
  path: string;
  label: string;
  icon: LucideIcon;
  badge?: string;
  category?: string;
  /** Omit to show to every role — set to restrict visibility (e.g. Timesheet). */
  roles?: EmployeeRole[];
}

export const NAV_ITEMS: NavItem[] = [
  { path: "/", label: "Dashboard", icon: LayoutDashboard },
  {
    path: "/timesheet",
    label: "Timesheet",
    icon: Clock,
    category: "Time & Leave",
    // Only the clocking report parser lives here now — HR only.
    roles: [EmployeeRole.HR],
  },
  { path: "/work-shift", label: "Work Shift", icon: CalendarClock, badge: "NEW", category: "Time & Leave" },
  { path: "/leave", label: "Leave Management", icon: CalendarOff, category: "Time & Leave" },
  { path: "/safety-flra", label: "Safety & FLRA", icon: ShieldCheck, category: "Safety & Compliance" },
  { path: "/incidents", label: "Incidents & Near Miss", icon: AlertTriangle, category: "Safety & Compliance" },
  { path: "/ppe", label: "PPE Management", icon: HardHat, category: "Safety & Compliance" },
  { path: "/legal-appointments", label: "Legal Appointments", icon: Scale, category: "Safety & Compliance" },
  { path: "/emergency", label: "Emergency (SOS)", icon: Siren, category: "Safety & Compliance" },
  { path: "/medical", label: "Medical", icon: HeartPulse, category: "People & Development" },
  { path: "/training", label: "Training & Certifications", icon: GraduationCap, category: "People & Development" },
  { path: "/performance", label: "Performance (KPI)", icon: TrendingUp, category: "People & Development" },
  { path: "/certificates", label: "My Certificates", icon: Award, category: "People & Development" },
  { path: "/policies", label: "Policies & Documents", icon: FileText, category: "Admin & Info" },
  { path: "/announcements", label: "Announcements", icon: Megaphone, category: "Admin & Info" },
  { path: "/reports", label: "Reports & Analytics", icon: BarChart3, category: "Admin & Info" },
  { path: "/settings", label: "Settings", icon: Settings, category: "Admin & Info" },
];

const CATEGORY_ORDER = ["Time & Leave", "Safety & Compliance", "People & Development", "Admin & Info"];

export interface NavGroup {
  label: string;
  items: NavItem[];
}

export const NAV_GROUPS: NavGroup[] = CATEGORY_ORDER.map((label) => ({
  label,
  items: NAV_ITEMS.filter((item) => item.category === label),
}));
