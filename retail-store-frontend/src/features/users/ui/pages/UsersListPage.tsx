import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { UserPlus, Users as UsersIcon } from 'lucide-react';
import { Button, Card, Spinner, EmptyState, SearchInput, FilterPillBar, PageHeader } from '@shared/components/ui';
import { useUsers } from '@features/users/application/hooks/useUsersQueries';
import { UserRow } from '@features/users/ui/components/UserRow';
import { RoleManagementModal } from '@features/users/ui/components/RoleManagementModal';
import { CreateUserModal } from '@features/users/ui/components/CreateUserModal';
import type { User } from '@features/users/domain/users.model';
 
export function UsersListPage() {
  const { t } = useTranslation();
  const { data: users, isLoading } = useUsers();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'inactive'>('all');
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [showCreate, setShowCreate] = useState(false);
 
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
      <PageHeader
        title={t('users.title')}
        subtitle={t('users.subtitle')}
        action={
          <Button onClick={() => setShowCreate(true)}>
            <UserPlus className="h-4 w-4" />
            {t('users.createUser')}
          </Button>
        }
      />
 
      {/* Filters */}
      <Card>
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
          <SearchInput
            value={search}
            onChange={setSearch}
            placeholder={`${t('common.search')} ${t('users.list').toLowerCase()}...`}
          />
          <FilterPillBar
            options={[
              { key: 'all', label: t('common.all') },
              { key: 'active', label: t('common.active') },
              { key: 'inactive', label: t('common.inactive') },
            ]}
            value={statusFilter}
            onChange={(v) => setStatusFilter(v as typeof statusFilter)}
          />
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
            description={t('users.noUsersDesc')}
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
 
      {/* Create User Modal */}
      <CreateUserModal isOpen={showCreate} onClose={() => setShowCreate(false)} />

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