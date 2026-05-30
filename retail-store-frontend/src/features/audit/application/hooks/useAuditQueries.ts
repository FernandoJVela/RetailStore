import { useQuery } from '@tanstack/react-query';
import { auditRepository } from '@features/audit/infrastructure/audit.repository';
 
const KEYS = {
  search: (params?: Record<string, unknown>) => ['audit', 'search', params] as const,
  detail: (id: string) => ['audit', id] as const,
  entity: (type: string, id: string) => ['audit', 'entity', type, id] as const,
  failures: ['audit', 'failures'] as const,
  moduleActivity: ['audit', 'module-activity'] as const,
  userActivity: ['audit', 'user-activity'] as const,
};
 
export function useAuditSearch(params?: {
  module?: string; outcome?: string; limit?: number;
}) {
  return useQuery({
    queryKey: KEYS.search(params),
    queryFn: () => auditRepository.search(params),
    staleTime: 15_000,
  });
}
 
export function useAuditDetail(id: string) {
  return useQuery({
    queryKey: KEYS.detail(id),
    queryFn: () => auditRepository.getById(id),
    enabled: !!id,
  });
}
 
export function useEntityHistory(entityType: string, entityId: string) {
  return useQuery({
    queryKey: KEYS.entity(entityType, entityId),
    queryFn: () => auditRepository.getEntityHistory(entityType, entityId),
    enabled: !!entityType && !!entityId,
  });
}
 
export function useAuditFailures(limit = 50) {
  return useQuery({
    queryKey: KEYS.failures,
    queryFn: () => auditRepository.getFailures(limit),
    staleTime: 15_000,
  });
}
 
export function useModuleActivity() {
  return useQuery({
    queryKey: KEYS.moduleActivity,
    queryFn: () => auditRepository.getModuleActivity(),
    staleTime: 30_000,
  });
}
 
export function useUserActivity() {
  return useQuery({
    queryKey: KEYS.userActivity,
    queryFn: () => auditRepository.getUserActivity(),
    staleTime: 30_000,
  });
}