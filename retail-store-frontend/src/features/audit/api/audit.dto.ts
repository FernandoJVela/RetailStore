/** API DTOs — match backend JSON responses exactly. */
 
export interface AuditEntryDto {
  id: string;
  timestamp: string;
  username: string | null;
  action: string;
  module: string;
  entityType: string | null;
  entityId: string | null;
  description: string | null;
  outcome: string;
  durationMs: number;
  correlationId: string | null;
}
 
export interface AuditDetailDto {
  id: string;
  timestamp: string;
  userId: string | null;
  username: string | null;
  ipAddress: string | null;
  action: string;
  module: string;
  entityType: string | null;
  entityId: string | null;
  description: string | null;
  requestPayload: string | null;
  responseSummary: string | null;
  changedProperties: string | null;
  outcome: string;
  errorCode: string | null;
  errorMessage: string | null;
  durationMs: number;
  correlationId: string | null;
  requestId: string | null;
}
 
export interface ModuleActivityDto {
  module: string;
  totalActions: number;
  successCount: number;
  failureCount: number;
  avgDurationMs: number;
  maxDurationMs: number;
  uniqueUsers: number;
  lastActivity: string | null;
}
 
export interface UserActivityDto {
  userId: string | null;
  username: string | null;
  totalActions: number;
  failedActions: number;
  modulesAccessed: number;
  firstAction: string | null;
  lastAction: string | null;
}