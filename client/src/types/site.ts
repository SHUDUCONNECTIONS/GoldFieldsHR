export interface SiteAdminDto {
  id: string;
  name: string;
  location: string;
  isActive: boolean;
  employeeCount: number;
}

export interface CreateSiteRequest {
  name: string;
  location: string;
}

export interface UpdateSiteRequest {
  name: string;
  location: string;
}

export interface SetSiteActiveStatusRequest {
  isActive: boolean;
}
