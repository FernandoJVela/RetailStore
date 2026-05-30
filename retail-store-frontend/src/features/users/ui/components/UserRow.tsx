import { useTranslation } from 'react-i18next';
import { Shield, UserX, UserCheck } from 'lucide-react';
import { Badge, ActionMenu, Avatar } from '@shared/components/ui';
import { formatDateTime } from '@shared/lib/utils';
import { useDeactivateUser, useReactivateUser } from '@features/users/application/hooks/useUsersQueries';
import type { User } from '@features/users/domain/users.model';

interface UserRowProps {
  user: User;
  onManageRoles: () => void;
}

export function UserRow({ user, onManageRoles }: UserRowProps) {
  const { t } = useTranslation();
  const deactivate = useDeactivateUser();
  const reactivate = useReactivateUser();

  const handleToggleActive = async () => {
    if (user.isActive) await deactivate.mutateAsync(user.id);
    else await reactivate.mutateAsync(user.id);
  };

  return (
    <tr className="group hover:bg-surface-50 dark:hover:bg-surface-800/50 transition-colors">
      <td className="py-3.5">
        <div className="flex items-center gap-3">
          <Avatar initials={user.username.charAt(0)} />
          <span className="font-medium text-[var(--text-primary)]">{user.username}</span>
        </div>
      </td>
      <td className="py-3.5 text-[var(--text-secondary)]">{user.email}</td>
      <td className="py-3.5">
        <div className="flex flex-wrap gap-1.5">
          {user.roles.length > 0 ? (
            user.roles.map((role) => (
              <Badge key={role} variant="info">{role}</Badge>
            ))
          ) : (
            <span className="text-[var(--text-muted)] text-xs italic">No roles</span>
          )}
        </div>
      </td>
      <td className="py-3.5">
        <Badge variant={user.isActive ? 'success' : 'danger'}>
          {user.isActive ? t('common.active') : t('common.inactive')}
        </Badge>
      </td>
      <td className="py-3.5 text-[var(--text-secondary)]">
        {user.lastLogin ? formatDateTime(user.lastLogin) : '—'}
      </td>
      <td className="py-3.5 text-right">
        <ActionMenu
          items={[
            {
              label: t('users.roles'),
              icon: Shield,
              onClick: onManageRoles,
            },
            {
              label: user.isActive ? t('users.deactivate') : t('users.reactivate'),
              icon: user.isActive ? UserX : UserCheck,
              onClick: handleToggleActive,
              variant: user.isActive ? 'danger' : 'success',
            },
          ]}
        />
      </td>
    </tr>
  );
}
