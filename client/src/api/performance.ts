import { apiClient } from "./client";
import type { CreatePerformanceReviewRequest, PerformanceReviewDto } from "../types/performance";

export async function createPerformanceReview(
  request: CreatePerformanceReviewRequest,
): Promise<PerformanceReviewDto> {
  const { data } = await apiClient.post<PerformanceReviewDto>("/performance", request);
  return data;
}

export async function getMyPerformanceReviews(): Promise<PerformanceReviewDto[]> {
  const { data } = await apiClient.get<PerformanceReviewDto[]>("/performance/mine");
  return data;
}

export async function getPerformanceReviewsGivenByMe(): Promise<PerformanceReviewDto[]> {
  const { data } = await apiClient.get<PerformanceReviewDto[]>("/performance/given");
  return data;
}

export async function getAllPerformanceReviews(): Promise<PerformanceReviewDto[]> {
  const { data } = await apiClient.get<PerformanceReviewDto[]>("/performance");
  return data;
}
