import { useTranslation } from 'react-i18next';
import { ShieldCheck, ShieldX } from 'lucide-react';
import { Modal, Button, Badge, Spinner } from '@shared/components/ui';
import { useRoles, useAssignRole, useRevokeRole } from '@features/users';
import type { User } from '@features/users';
 
interface RoleManagementModalProps {
  user: User;
  isOpen: boolean;
  onClose: () => void;
}
 
/**
 * Safely extract a displayable string from a permission.
 * Permissions from the backend can be:
 *   - A plain string: "products:read"
 *   - An object: { resource: "products", action: "read", fullName: "products:read" }
 * This function handles both cases.
 */
function formatPermission(perm: unknown): string {
  if (typeof perm === 'string') return perm;
  if (perm && typeof perm === 'object') {
    const obj = perm as Record<string, unknown>;
    // Try fullName first, then resource:action, then JSON
    if (typeof obj.fullName === 'string') return obj.fullName;
    if (typeof obj.resource === 'string' && typeof obj.action === 'string')
      return `${obj.resource}:${obj.action}`;
    return JSON.stringify(perm);
  }
  return String(perm);
}
 
export function RoleManagementModal({ user, isOpen, onClose }: RoleManagementModalProps) {
  const { t } = useTranslation();
  const { data: roles, isLoading } = useRoles();
  const assignRole = useAssignRole();
  const revokeRole = useRevokeRole();
 
  const handleAssign = async (roleName: string) => {
    await assignRole.mutateAsync({ userId: user.id, roleName });
  };
 
  const handleRevoke = async (roleName: string) => {
    await revokeRole.mutateAsync({ userId: user.id, roleName });
  };
 
  const userHasRole = (roleName: string) => user.roles.includes(roleName);
 
  return (
    <Modal isOpen={isOpen} onClose={onClose} title={`${t('users.roles')} — ${user.username}`} size="lg">
      {isLoading ? (
        <Spinner />
      ) : (
        <div className="space-y-3">
          {roles?.map((role) => {
            const hasRole = userHasRole(role.name);
            // Format permissions safely — handles both string[] and object[]
            const permStrings = role.permissions.map(formatPermission);
 
            return (
              <div
                key={role.id}
                className={`flex items-center justify-between rounded-lg border p-4 transition-colors ${
                  hasRole
                    ? 'border-primary-300 bg-primary-50 dark:border-primary-700 dark:bg-primary-500/10'
                    : 'border-[var(--border-color)] bg-[var(--bg-primary)]'
                }`}
              >
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <h4 className="font-medium text-[var(--text-primary)]">{role.name}</h4>
                    {hasRole && <Badge variant="success">Assigned</Badge>}
                    {role.isSystem && <Badge variant="default">System</Badge>}
                  </div>
                  <p className="mt-1 text-sm text-[var(--text-secondary)]">{role.description}</p>
                  <div className="mt-2 flex flex-wrap gap-1">
                    {permStrings.slice(0, 5).map((perm, idx) => (
                      <span
                        key={`${role.id}-perm-${idx}`}
                        className="rounded bg-[var(--bg-tertiary)] px-1.5 py-0.5 text-xs text-[var(--text-secondary)]"
                      >
                        {perm}
                      </span>
                    ))}
                    {permStrings.length > 5 && (
                      <span className="text-xs text-[var(--text-muted)]">
                        +{permStrings.length - 5} more
                      </span>
                    )}
                  </div>
                </div>
                <div className="ml-4 shrink-0">
                  {hasRole ? (
                    <Button
                      variant="danger"
                      size="sm"
                      onClick={() => handleRevoke(role.name)}
                      loading={revokeRole.isPending}
                    >
                      <ShieldX className="h-4 w-4" />
                      {t('users.revokeRole')}
                    </Button>
                  ) : (
                    <Button
                      variant="primary"
                      size="sm"
                      onClick={() => handleAssign(role.name)}
                      loading={assignRole.isPending}
                    >
                      <ShieldCheck className="h-4 w-4" />
                      {t('users.assignRole')}
                    </Button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}
      <div className="mt-6 flex justify-end">
        <Button variant="secondary" onClick={onClose}>{t('common.cancel')}</Button>
      </div>
    </Modal>
  );
}