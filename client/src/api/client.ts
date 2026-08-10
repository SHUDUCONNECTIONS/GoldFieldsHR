import axios from "axios";

export const TOKEN_STORAGE_KEY = "goldfields.accessToken";
export const REFRESH_TOKEN_STORAGE_KEY = "goldfields.refreshToken";
export const SESSION_EXPIRED_EVENT = "auth:session-expired";

export const apiClient = axios.create({
  baseURL: "/api",
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_STORAGE_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

let isRefreshing = false;
let pendingRequests: Array<(accessToken: string | null) => void> = [];

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as (typeof error.config & { _retry?: boolean }) | undefined;

    const isAuthEndpoint = originalRequest?.url?.startsWith("/auth/");
    if (!axios.isAxiosError(error) || error.response?.status !== 401 || !originalRequest || originalRequest._retry || isAuthEndpoint) {
      return Promise.reject(error);
    }

    const refreshToken = localStorage.getItem(REFRESH_TOKEN_STORAGE_KEY);
    if (!refreshToken) {
      window.dispatchEvent(new Event(SESSION_EXPIRED_EVENT));
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    if (!isRefreshing) {
      isRefreshing = true;
      try {
        const { data } = await axios.post(`${apiClient.defaults.baseURL}/auth/refresh`, { refreshToken });
        localStorage.setItem(TOKEN_STORAGE_KEY, data.accessToken);
        localStorage.setItem(REFRESH_TOKEN_STORAGE_KEY, data.refreshToken);
        pendingRequests.forEach((resolve) => resolve(data.accessToken));
      } catch {
        pendingRequests.forEach((resolve) => resolve(null));
        window.dispatchEvent(new Event(SESSION_EXPIRED_EVENT));
      } finally {
        pendingRequests = [];
        isRefreshing = false;
      }
    }

    return new Promise((resolve, reject) => {
      pendingRequests.push((accessToken) => {
        if (!accessToken) {
          reject(error);
          return;
        }
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        resolve(apiClient(originalRequest));
      });
    });
  },
);

export function extractErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const errors = error.response?.data?.errors as string[] | undefined;
    if (errors?.length) return errors.join(" ");
    const singleError = error.response?.data?.error as string | undefined;
    if (singleError) return singleError;
    if (error.message) return error.message;
  }
  return "Something went wrong. Please try again.";
}
