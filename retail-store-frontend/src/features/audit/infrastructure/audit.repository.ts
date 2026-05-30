import { auditApi } from '@features/audit';
import type { AuditEntry, AuditDetail, ModuleActivity, UserActivity } from '@features/audit';
import { mapAuditEntryDto, mapAuditDetailDto, mapModuleActivityDto, mapUserActivityDto } from '@features/audit/application/mappers/audit.mapper';
 
export const auditRepository = {
  async search(params?: {
    userId?: string; module?: string; entityType?: string;
    entityId?: string; outcome?: string;
    from?: string; to?: string; limit?: number;
  }): Promise<AuditEntry[]> {
    const { data } = await auditApi.search(params);
    return data.map(mapAuditEntryDto);
  },
 
  async getById(id: string): Promise<AuditDetail> {
    const { data } = await auditApi.getById(id);
    return mapAuditDetailDto(data);
  },
 
  async getEntityHistory(entityType: string, entityId: string): Promise<AuditEntry[]> {
    const { data } = await auditApi.getEntityHistory(entityType, entityId);
    return data.map(mapAuditEntryDto);
  },
 
  async getFailures(limit = 50): Promise<AuditEntry[]> {
    const { data } = await auditApi.getFailures(limit);
    return data.map(mapAuditEntryDto);
  },
 
  async getModuleActivity(): Promise<ModuleActivity[]> {
    const { data } = await auditApi.getModuleActivity();
    return data.map(mapModuleActivityDto);
  },
 
  async getUserActivity(): Promise<UserActivity[]> {
    const { data } = await auditApi.getUserActivity();
    return data.map(mapUserActivityDto);
  },
};