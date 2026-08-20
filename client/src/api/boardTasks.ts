import { apiClient } from "./client";
import type {
  BoardTaskDto,
  ChangeTaskStatusRequest,
  CreateBoardTaskRequest,
  UpdateBoardTaskRequest,
  WeeklyTaskCompletionDto,
} from "../types/board";

export async function getTasksForBoard(boardId: string): Promise<BoardTaskDto[]> {
  const { data } = await apiClient.get<BoardTaskDto[]>(`/boards/${boardId}/tasks`);
  return data;
}

export async function createBoardTask(boardId: string, request: CreateBoardTaskRequest): Promise<BoardTaskDto> {
  const { data } = await apiClient.post<BoardTaskDto>(`/boards/${boardId}/tasks`, request);
  return data;
}

export async function getBoardTaskById(boardId: string, taskId: string): Promise<BoardTaskDto> {
  const { data } = await apiClient.get<BoardTaskDto>(`/boards/${boardId}/tasks/${taskId}`);
  return data;
}

export async function updateBoardTask(
  boardId: string,
  taskId: string,
  request: UpdateBoardTaskRequest,
): Promise<BoardTaskDto> {
  const { data } = await apiClient.patch<BoardTaskDto>(`/boards/${boardId}/tasks/${taskId}`, request);
  return data;
}

export async function changeBoardTaskStatus(
  boardId: string,
  taskId: string,
  request: ChangeTaskStatusRequest,
): Promise<BoardTaskDto> {
  const { data } = await apiClient.post<BoardTaskDto>(`/boards/${boardId}/tasks/${taskId}/status`, request);
  return data;
}

export async function deleteBoardTask(boardId: string, taskId: string): Promise<void> {
  await apiClient.delete(`/boards/${boardId}/tasks/${taskId}`);
}

export async function getWeeklySummary(boardId: string, weekStartUtc?: string): Promise<WeeklyTaskCompletionDto[]> {
  const { data } = await apiClient.get<WeeklyTaskCompletionDto[]>(`/boards/${boardId}/tasks/weekly-summary`, {
    params: weekStartUtc ? { weekStartUtc } : undefined,
  });
  return data;
}

export async function downloadWeeklySummaryPdf(boardId: string, boardName: string): Promise<void> {
  const response = await apiClient.get(`/boards/${boardId}/tasks/weekly-summary/pdf`, { responseType: "blob" });
  const url = URL.createObjectURL(response.data as Blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = `weekly-summary-${boardName}.pdf`;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}
