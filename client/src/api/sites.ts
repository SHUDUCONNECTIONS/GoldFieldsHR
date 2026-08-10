import { apiClient } from "./client";
import type { CreateSiteRequest, SetSiteActiveStatusRequest, SiteAdminDto, UpdateSiteRequest } from "../types/site";

export async function getAllSites(): Promise<SiteAdminDto[]> {
  const { data } = await apiClient.get<SiteAdminDto[]>("/sites/all");
  return data;
}

export async function createSite(request: CreateSiteRequest): Promise<SiteAdminDto> {
  const { data } = await apiClient.post<SiteAdminDto>("/sites", request);
  return data;
}

export async function updateSite(id: string, request: UpdateSiteRequest): Promise<SiteAdminDto> {
  const { data } = await apiClient.put<SiteAdminDto>(`/sites/${id}`, request);
  return data;
}

export async function setSiteActiveStatus(id: string, request: SetSiteActiveStatusRequest): Promise<SiteAdminDto> {
  const { data } = await apiClient.patch<SiteAdminDto>(`/sites/${id}/status`, request);
  return data;
}
