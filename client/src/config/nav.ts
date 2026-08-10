import {
  LayoutDashboard,
  Clock,
  CalendarClock,
  CalendarOff,
  ShieldCheck,
  AlertTriangle,
  FileText,
  HeartPulse,
  GraduationCap,
  HardHat,
  ClipboardList,
  TrendingUp,
  Award,
  BarChart3,
  Siren,
  Settings,
  type LucideIcon,
} from "lucide-react";

export interface NavItem {
  path: string;
  label: string;
  icon: LucideIcon;
  badge?: string;
}

export const NAV_ITEMS: NavItem[] = [
  { path: "/", label: "Dashboard", icon: LayoutDashboard },
  { path: "/timesheet", label: "Timesheet", icon: Clock },
  { path: "/work-shift", label: "Work Shift", icon: CalendarClock, badge: "NEW" },
  { path: "/leave", label: "Leave Management", icon: CalendarOff },
  { path: "/safety-flra", label: "Safety & FLRA", icon: ShieldCheck },
  { path: "/incidents", label: "Incidents & Near Miss", icon: AlertTriangle },
  { path: "/policies", label: "Policies & Documents", icon: FileText },
  { path: "/medical", label: "Medical", icon: HeartPulse },
  { path: "/training", label: "Training & Certifications", icon: GraduationCap },
  { path: "/ppe", label: "PPE Management", icon: HardHat },
  { path: "/permits", label: "Permits to Work", icon: ClipboardList },
  { path: "/performance", label: "Performance (KPI)", icon: TrendingUp },
  { path: "/certificates", label: "My Certificates", icon: Award },
  { path: "/reports", label: "Reports & Analytics", icon: BarChart3 },
  { path: "/emergency", label: "Emergency (SOS)", icon: Siren },
  { path: "/settings", label: "Settings", icon: Settings },
];
