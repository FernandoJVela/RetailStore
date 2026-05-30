import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Building2 } from 'lucide-react';
import { Button, Card, Spinner, EmptyState, SearchInput, FilterPillBar, PageHeader } from '@shared/components/ui';
import { useDebounce } from '@shared/hooks';
import { 
    ProviderRow, 
    useProviders, 
    RegisterProviderModal, 
    ProviderDetailPanel } from '@features/providers';
 
export function ProvidersListPage() {
  const { t } = useTranslation();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'inactive'>('all');
  const [showRegister, setShowRegister] = useState(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);
 
  const debouncedSearch = useDebounce(search, 300);
  const { data: providers, isLoading } = useProviders({
    search: debouncedSearch || undefined,
    isActive: statusFilter === 'all' ? undefined : statusFilter === 'active',
  });
 
  return (
    <div className="space-y-6">
      {/* Header */}
      <PageHeader
        title={t('nav.providers')}
        subtitle={t('providers.subtitle', { count: providers?.length ?? 0 })}
        action={
          <Button onClick={() => setShowRegister(true)}>
            <Plus className="h-4 w-4" />
            <span className="hidden sm:inline">{t('providers.registerProvider')}</span>
            <span className="sm:hidden">{t('common.new')}</span>
          </Button>
        }
      />
 
      {/* Filters */}
      <Card>
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
          <SearchInput
            value={search}
            onChange={setSearch}
            placeholder={t('providers.searchPlaceholder')}
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
 
      {/* Table */}
      <Card>
        {isLoading ? (
          <Spinner />
        ) : !providers?.length ? (
          <EmptyState
            icon={<Building2 className="h-12 w-12" />}
            title={t('providers.noProvidersFound')}
            description={t('providers.noProvidersDesc')}
            action={<Button onClick={() => setShowRegister(true)}>{t('providers.registerProvider')}</Button>}
          />
        ) : (
          <div className="overflow-x-auto -mx-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('providers.col_company')}</th>
                  <th className="hidden md:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('providers.col_contact')}</th>
                  <th className="hidden lg:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('users.email')}</th>
                  <th className="px-6 pb-3 text-center font-medium text-[var(--text-secondary)]">{t('providers.col_products')}</th>
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('common.status')}</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('common.actions')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--border-color)]">
                {providers.map((provider) => (
                  <ProviderRow
                    key={provider.id}
                    provider={provider}
                    onViewDetail={() => setSelectedId(provider.id)}
                  />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
 
      {/* Modals */}
      <RegisterProviderModal isOpen={showRegister} onClose={() => setShowRegister(false)} />
      {selectedId && (
        <ProviderDetailPanel
          providerId={selectedId}
          isOpen={!!selectedId}
          onClose={() => setSelectedId(null)}
        />
      )}
    </div>
  );
}