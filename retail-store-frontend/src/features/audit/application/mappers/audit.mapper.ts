import type { AuditEntryDto, AuditDetailDto, ModuleActivityDto, UserActivityDto } from '@features/audit';
import type { AuditEntry, AuditDetail, PropertyChange, ModuleActivity, UserActivity, AuditOutcome } from '@features/audit';
 
function timeAgo(date: Date): string {
  const mins = Math.floor((Date.now() - date.getTime()) / 60_000);
  if (mins < 1) return 'Just now';
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  return `${days}d ago`;
}
 
/** "CreateProductCommand" → "Create Product" */
function formatAction(action: string): string {
  return action
    .replace('Command', '')
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .trim();
}
 
function formatDuration(ms: number): string {
  if (ms < 1000) return `${ms}ms`;
  return `${(ms / 1000).toFixed(1)}s`;
}
 
function tryParseJson<T>(json: string | null): T | null {
  if (!json) return null;
  try { return JSON.parse(json) as T; }
  catch { return null; }
}
 
export function mapAuditEntryDto(dto: AuditEntryDto): AuditEntry {
  const ts = new Date(dto.timestamp);
  const outcome = dto.outcome as AuditOutcome;
  return {
    id: dto.id,
    timestamp: ts,
    timeAgo: timeAgo(ts),
    username: dto.username ?? 'System',
    action: dto.action,
    actionLabel: formatAction(dto.action),
    module: dto.module,
    entityType: dto.entityType,
    entityId: dto.entityId,
    entityIdShort: dto.entityId?.substring(0, 8) ?? null,
    description: dto.description,
    outcome,
    isSuccess: outcome === 'Success',
    durationMs: dto.durationMs,
    durationLabel: formatDuration(dto.durationMs),
    correlationId: dto.correlationId,
  };
}
 
export function mapAuditDetailDto(dto: AuditDetailDto): AuditDetail {
  const base = mapAuditEntryDto(dto);
  return {
    ...base,
    userId: dto.userId,
    ipAddress: dto.ipAddress,
    requestPayload: tryParseJson<Record<string, unknown>>(dto.requestPayload),
    responseSummary: dto.responseSummary,
    changedProperties: tryParseJson<PropertyChange[]>(dto.changedProperties),
    errorCode: dto.errorCode,
    errorMessage: dto.errorMessage,
    requestId: dto.requestId,
  };
}
 
export function mapModuleActivityDto(dto: ModuleActivityDto): ModuleActivity {
  return {
    module: dto.module,
    totalActions: dto.totalActions,
    successCount: dto.successCount,
    failureCount: dto.failureCount,
    successRate: dto.totalActions > 0 ? Math.round((dto.successCount / dto.totalActions) * 100) : 0,
    avgDurationMs: dto.avgDurationMs,
    maxDurationMs: dto.maxDurationMs,
    uniqueUsers: dto.uniqueUsers,
    lastActivity: dto.lastActivity ? new Date(dto.lastActivity) : null,
  };
}
 
export function mapUserActivityDto(dto: UserActivityDto): UserActivity {
  return {
    userId: dto.userId,
    username: dto.username ?? 'System',
    totalActions: dto.totalActions,
    failedActions: dto.failedActions,
    failureRate: dto.totalActions > 0 ? Math.round((dto.failedActions / dto.totalActions) * 100) : 0,
    modulesAccessed: dto.modulesAccessed,
    firstAction: dto.firstAction ? new Date(dto.firstAction) : null,
    lastAction: dto.lastAction ? new Date(dto.lastAction) : null,
  };
}