import { httpClient } from '@shared/api/http-client';
import type {
  AuditEntryDto, AuditDetailDto, ModuleActivityDto, UserActivityDto,
} from './audit.dto';
 
const BASE = '/audit';
 
export const auditApi = {
  search: (params?: {
    userId?: string; module?: string; entityType?: string;
    entityId?: string; outcome?: string;
    from?: string; to?: string; limit?: number;
  }) =>
    httpClient.get<AuditEntryDto[]>(BASE, { params }),
 
  getById: (id: string) =>
    httpClient.get<AuditDetailDto>(`${BASE}/${id}`),
 
  getEntityHistory: (entityType: string, entityId: string) =>
    httpClient.get<AuditEntryDto[]>(`${BASE}/entity/${entityType}/${entityId}`),
 
  getFailures: (limit = 50) =>
    httpClient.get<AuditEntryDto[]>(`${BASE}/failures`, { params: { limit } }),
 
  getModuleActivity: () =>
    httpClient.get<ModuleActivityDto[]>(`${BASE}/activity/by-module`),
 
  getUserActivity: () =>
    httpClient.get<UserActivityDto[]>(`${BASE}/activity/by-user`),
};