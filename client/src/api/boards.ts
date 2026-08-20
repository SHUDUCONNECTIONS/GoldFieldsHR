import { apiClient } from "./client";
import type { AddBoardMemberRequest, BoardDto, CreateBoardRequest, UpdateBoardRequest } from "../types/board";

export async function getMyBoards(): Promise<BoardDto[]> {
  const { data } = await apiClient.get<BoardDto[]>("/boards/mine");
  return data;
}

export async function createBoard(request: CreateBoardRequest): Promise<BoardDto> {
  const { data } = await apiClient.post<BoardDto>("/boards", request);
  return data;
}

export async function getBoardById(id: string): Promise<BoardDto> {
  const { data } = await apiClient.get<BoardDto>(`/boards/${id}`);
  return data;
}

export async function updateBoard(id: string, request: UpdateBoardRequest): Promise<BoardDto> {
  const { data } = await apiClient.patch<BoardDto>(`/boards/${id}`, request);
  return data;
}

export async function addBoardMember(id: string, request: AddBoardMemberRequest): Promise<BoardDto> {
  const { data } = await apiClient.post<BoardDto>(`/boards/${id}/members`, request);
  return data;
}

export async function removeBoardMember(id: string, employeeId: string): Promise<BoardDto> {
  const { data } = await apiClient.delete<BoardDto>(`/boards/${id}/members/${employeeId}`);
  return data;
}
