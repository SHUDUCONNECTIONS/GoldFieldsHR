import { useAuth } from "../auth/AuthContext";
import { ClockingReportParser } from "../components/ClockingReportParser";
import { EmployeeRole } from "../types/auth";

export function TimesheetPage() {
  const { session } = useAuth();
  const isHR = session?.role === EmployeeRole.HR;

  return <div className="stagger-children flex flex-col gap-6">{isHR && <ClockingReportParser />}</div>;
}
