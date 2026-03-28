import { useTranslation } from 'react-i18next';
import { MoreHorizontal, Shield, UserX, UserCheck } from 'lucide-react';
import { Badge } from '@shared/components/ui';
import { formatDateTime } from '@shared/lib/utils';
import { useDeactivateUser, useReactivateUser } from '@features/users/application/hooks/useUsersQueries';
import type { User } from '@features/users/domain/users.model';
import { useState, useRef, useEffect } from 'react';
 
interface UserRowProps {
  user: User;
  onManageRoles: () => void;
}
 
export function UserRow({ user, onManageRoles }: UserRowProps) {
  const { t } = useTranslation();
  const deactivate = useDeactivateUser();
  const reactivate = useReactivateUser();
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
 
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) setMenuOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);
 
  const handleToggleActive = async () => {
    setMenuOpen(false);
    if (user.isActive) {
      await deactivate.mutateAsync(user.id);
    } else {
      await reactivate.mutateAsync(user.id);
    }
  };
 
  return (
    <tr className="group hover:bg-surface-50 dark:hover:bg-surface-800/50 transition-colors">
      <td className="py-3.5">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-full bg-primary-100 dark:bg-primary-900/30 text-sm font-bold text-primary-700 dark:text-primary-400">
            {user.username.charAt(0).toUpperCase()}
          </div>
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
        <div className="relative" ref={menuRef}>
          <button
            onClick={() => setMenuOpen(!menuOpen)}
            className="rounded-lg p-1.5 text-[var(--text-muted)] hover:bg-surface-200 dark:hover:bg-surface-700 transition-colors"
          >
            <MoreHorizontal className="h-5 w-5" />
          </button>
 
          {menuOpen && (
            <div className="absolute right-0 top-full z-10 mt-1 w-48 rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] py-1 shadow-lg">
              <button
                onClick={() => { onManageRoles(); setMenuOpen(false); }}
                className="flex w-full items-center gap-2 px-4 py-2.5 text-sm text-[var(--text-primary)] hover:bg-surface-100 dark:hover:bg-surface-800"
              >
                <Shield className="h-4 w-4" />
                {t('users.roles')}
              </button>
              <button
                onClick={handleToggleActive}
                className={`flex w-full items-center gap-2 px-4 py-2.5 text-sm ${
                  user.isActive
                    ? 'text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20'
                    : 'text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20'
                }`}
              >
                {user.isActive ? (
                  <><UserX className="h-4 w-4" /> {t('users.deactivate')}</>
                ) : (
                  <><UserCheck className="h-4 w-4" /> {t('users.reactivate')}</>
                )}
              </button>
            </div>
          )}
        </div>
      </td>
    </tr>
  );
}