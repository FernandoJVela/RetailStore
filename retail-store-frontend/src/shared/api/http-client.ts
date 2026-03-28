import axios, { type InternalAxiosRequestConfig, type AxiosError } from 'axios';
 
const API_BASE_URL = '/api/v1';
 
export const httpClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 30000,
});
 
// ─── Request interceptor: attach JWT token ──────────────────
httpClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);
 
// ─── Response interceptor: handle 401 + transform errors ────
httpClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiProblemDetails>) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
 
// ─── API error type (matches backend ProblemDetails) ────────
export interface ApiProblemDetails {
  type: string;
  title: string;
  status: number;
  detail: string;
  errorCode: string;
  traceId?: string;
  correlationId?: string;
  requestId?: string;
  timestamp?: string;
}
 
export function getApiErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error) && error.response?.data) {
    const problem = error.response.data as ApiProblemDetails;
    return problem.detail || problem.title || 'An unexpected error occurred';
  }
  return 'Network error. Please check your connection.';
}
 
export function getApiErrorCode(error: unknown): string | null {
  if (axios.isAxiosError(error) && error.response?.data) {
    return (error.response.data as ApiProblemDetails).errorCode || null;
  }
  return null;
}