/** Domain models — what the UI actually needs. */
 
export type AuditOutcome = 'Success' | 'Failure' | 'Error';
 
export interface AuditEntry {
  id: string;
  timestamp: Date;
  timeAgo: string;
  username: string;       // "System" if null
  action: string;
  actionLabel: string;    // Computed: "CreateProductCommand" → "Create Product"
  module: string;
  entityType: string | null;
  entityId: string | null;
  entityIdShort: string | null;  // First 8 chars
  description: string | null;
  outcome: AuditOutcome;
  isSuccess: boolean;
  durationMs: number;
  durationLabel: string;  // Computed: "23ms" or "1.2s"
  correlationId: string | null;
}
 
export interface AuditDetail extends AuditEntry {
  userId: string | null;
  ipAddress: string | null;
  requestPayload: Record<string, unknown> | null;  // Parsed JSON
  responseSummary: string | null;
  changedProperties: PropertyChange[] | null;       // Parsed JSON
  errorCode: string | null;
  errorMessage: string | null;
  requestId: string | null;
}
 
export interface PropertyChange {
  Property: string;
  OldValue: string;
  NewValue: string;
}
 
export interface ModuleActivity {
  module: string;
  totalActions: number;
  successCount: number;
  failureCount: number;
  successRate: number;     // Computed
  avgDurationMs: number;
  maxDurationMs: number;
  uniqueUsers: number;
  lastActivity: Date | null;
}
 
export interface UserActivity {
  userId: string | null;
  username: string;
  totalActions: number;
  failedActions: number;
  failureRate: number;     // Computed
  modulesAccessed: number;
  firstAction: Date | null;
  lastAction: Date | null;
}
 
/** Outcome badge variant */
export function outcomeVariant(outcome: AuditOutcome): 'success' | 'danger' | 'warning' {
  switch (outcome) {
    case 'Success': return 'success';
    case 'Failure': return 'danger';
    case 'Error': return 'danger';
  }
}
 
/** All modules for filter dropdown */
export const AUDIT_MODULES = [
  'Products', 'Orders', 'Customers', 'Inventory', 'Providers',
  'Users', 'Shipping', 'Notifications', 'Payments', 'System',
] as const;