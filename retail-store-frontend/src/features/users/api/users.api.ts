import { httpClient } from '@shared/api/http-client';
import type {
  LoginRequestDto, LoginResponseDto, RegisterRequestDto,
  UserDto, RoleDto,
} from '../index';
 
const BASE = '/users';
 
/** Raw API calls. No business logic. No mapping. Just HTTP. */
export const usersApi = {
  // ─── Auth ───────────────────────────────────────────────
  login: (data: LoginRequestDto) =>
    httpClient.post<LoginResponseDto>(`${BASE}/login`, data),
 
  register: (data: RegisterRequestDto) =>
    httpClient.post<string>(`${BASE}/register`, data),
 
  // ─── Users CRUD ─────────────────────────────────────────
  getAll: () =>
    httpClient.get<UserDto[]>(BASE),
 
  getById: (id: string) =>
    httpClient.get<UserDto>(`${BASE}/${id}`),
 
  deactivate: (id: string) =>
    httpClient.put(`${BASE}/${id}/deactivate`),
 
  reactivate: (id: string) =>
    httpClient.put(`${BASE}/${id}/reactivate`),
 
  // ─── Roles ──────────────────────────────────────────────
  getRoles: () =>
    httpClient.get<RoleDto[]>(`${BASE}/roles`),
 
  assignRole: (userId: string, roleId: string) =>
    httpClient.post(`${BASE}/${userId}/roles`, { roleId }),

  revokeRole: (userId: string, roleId: string) =>
    httpClient.delete(`${BASE}/${userId}/roles/${roleId}`),
};