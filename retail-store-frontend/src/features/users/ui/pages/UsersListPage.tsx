import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, UserPlus, Users as UsersIcon } from 'lucide-react';
import { Button, Card, Spinner, EmptyState } from '@shared/components/ui';
import { useUsers } from '@features/users/application/hooks/useUsersQueries';
import { UserRow } from '@features/users/ui/components/UserRow';
import { RoleManagementModal } from '@features/users/ui/components/RoleManagementModal';
import type { User } from '@features/users/domain/users.model';
 
export function UsersListPage() {
  const { t } = useTranslation();
  const { data: users, isLoading } = useUsers();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'inactive'>('all');
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
 
  const filtered = users?.filter((u) => {
    const matchesSearch =
      u.username.toLowerCase().includes(search.toLowerCase()) ||
      u.email.toLowerCase().includes(search.toLowerCase());
    const matchesStatus =
      statusFilter === 'all' ||
      (statusFilter === 'active' && u.isActive) ||
      (statusFilter === 'inactive' && !u.isActive);
    return matchesSearch && matchesStatus;
  });
 
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-[var(--text-primary)]">{t('users.title')}</h1>
          <p className="mt-1 text-sm text-[var(--text-secondary)]">{t('users.subtitle')}</p>
        </div>
        <Button>
          <UserPlus className="h-4 w-4" />
          {t('users.createUser')}
        </Button>
      </div>
 
      {/* Filters */}
      <Card>
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-[var(--text-muted)]" />
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder={`${t('common.search')} ${t('users.list').toLowerCase()}...`}
              className="w-full rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] py-2.5 pl-10 pr-4 text-sm text-[var(--text-primary)] placeholder:text-[var(--text-muted)] focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div className="flex gap-2">
            {(['all', 'active', 'inactive'] as const).map((status) => (
              <button
                key={status}
                onClick={() => setStatusFilter(status)}
                className={`rounded-lg px-4 py-2 text-sm font-medium transition-colors ${
                  statusFilter === status
                    ? 'bg-primary-600 text-white'
                    : 'bg-[var(--bg-primary)] text-[var(--text-secondary)] hover:bg-surface-200 dark:hover:bg-surface-700'
                }`}
              >
                {status === 'all' ? 'All' : status === 'active' ? t('common.active') : t('common.inactive')}
              </button>
            ))}
          </div>
        </div>
      </Card>
 
      {/* Users Table */}
      <Card>
        {isLoading ? (
          <Spinner />
        ) : !filtered?.length ? (
          <EmptyState
            icon={<UsersIcon className="h-12 w-12" />}
            title={t('users.noUsers')}
            description="Try adjusting your search or filter."
          />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="pb-3 text-left font-medium text-[var(--text-secondary)]">{t('users.username')}</th>
                  <th className="pb-3 text-left font-medium text-[var(--text-secondary)]">{t('users.email')}</th>
                  <th className="pb-3 text-left font-medium text-[var(--text-secondary)]">{t('users.roles')}</th>
                  <th className="pb-3 text-left font-medium text-[var(--text-secondary)]">{t('common.status')}</th>
                  <th className="pb-3 text-left font-medium text-[var(--text-secondary)]">{t('users.lastLogin')}</th>
                  <th className="pb-3 text-right font-medium text-[var(--text-secondary)]">{t('common.actions')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--border-color)]">
                {filtered.map((user) => (
                  <UserRow
                    key={user.id}
                    user={user}
                    onManageRoles={() => setSelectedUser(user)}
                  />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
 
      {/* Role Management Modal */}
      {selectedUser && (
        <RoleManagementModal
          user={selectedUser}
          isOpen={!!selectedUser}
          onClose={() => setSelectedUser(null)}
        />
      )}
    </div>
  );
}