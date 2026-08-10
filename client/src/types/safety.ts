export interface PreShiftSafetyCheck {
  id: string;
  employeeId: string;
  employeeName: string;
  checkDate: string;
  hazardsIdentified: boolean;
  hazardNotes: string | null;
  submittedAtUtc: string;
}

export interface SubmitPreShiftCheckRequest {
  hazardsIdentified: boolean;
  hazardNotes?: string;
}
