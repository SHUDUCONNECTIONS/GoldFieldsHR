export const PerformanceRange = {
  Week: 0,
  Month: 1,
  All: 2,
} as const;

export type PerformanceRange = (typeof PerformanceRange)[keyof typeof PerformanceRange];

export const PerformanceRangeLabels: Record<PerformanceRange, string> = {
  [PerformanceRange.Week]: "This Week",
  [PerformanceRange.Month]: "This Month",
  [PerformanceRange.All]: "All Time",
};

export interface PerformanceChartPointDto {
  label: string;
  bucketStart: string;
  tasksCompleted: number;
}

export interface MyPerformanceDto {
  tasksCompletedTotal: number;
  tasksInProgress: number;
  tasksOverdue: number;
  chart: PerformanceChartPointDto[];
}

export interface EmployeePerformanceDto {
  employeeId: string;
  employeeName: string;
  siteName: string;
  tasksCompleted: number;
  tasksInProgress: number;
  tasksOverdue: number;
}
