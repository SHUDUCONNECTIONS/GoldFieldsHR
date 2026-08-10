export const EmployeeRole = {
  Employee: 0,
  LineManager: 1,
  HR: 2,
  SafetyOfficer: 3,
  Medical: 4,
  Security: 5,
  Executive: 6,
} as const;

export type EmployeeRole = (typeof EmployeeRole)[keyof typeof EmployeeRole];

export const EmployeeRoleLabels: Record<EmployeeRole, string> = {
  [EmployeeRole.Employee]: "Employee",
  [EmployeeRole.LineManager]: "Line Manager",
  [EmployeeRole.HR]: "HR",
  [EmployeeRole.SafetyOfficer]: "Safety Officer",
  [EmployeeRole.Medical]: "Medical",
  [EmployeeRole.Security]: "Security",
  [EmployeeRole.Executive]: "Executive",
};

export interface Site {
  id: string;
  name: string;
  location: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  employeeNumber: string;
  jobTitle: string;
  role: EmployeeRole;
  siteId: string;
  managerEmployeeNumber?: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  employeeId: string;
  fullName: string;
  role: EmployeeRole;
  refreshToken: string;
}
