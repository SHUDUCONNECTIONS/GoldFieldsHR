export interface PerformanceReviewDto {
  id: string;
  employeeId: string;
  employeeName: string;
  reviewerId: string;
  reviewerName: string;
  periodLabel: string;
  score: number;
  comments: string | null;
  createdAtUtc: string;
}

export interface CreatePerformanceReviewRequest {
  employeeNumber: string;
  periodLabel: string;
  score: number;
  comments?: string;
}
