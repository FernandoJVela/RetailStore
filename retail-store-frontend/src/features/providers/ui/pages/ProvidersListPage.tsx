import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, Plus, Building2 } from 'lucide-react';
import { Button, Card, Spinner, EmptyState } from '@shared/components/ui';
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
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-[var(--text-primary)]">{t('nav.providers')}</h1>
          <p className="mt-1 text-sm text-[var(--text-secondary)]">
            {providers?.length ?? 0} suppliers registered
          </p>
        </div>
        <Button onClick={() => setShowRegister(true)}>
          <Plus className="h-4 w-4" />
          <span className="hidden sm:inline">Register Provider</span>
          <span className="sm:hidden">New</span>
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
              placeholder="Search by company, contact, or email..."
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
                    : 'bg-[var(--bg-primary)] text-[var(--text-secondary)] hover:bg-[var(--bg-tertiary)]'
                }`}
              >
                {status === 'all' ? 'All' : status === 'active' ? t('common.active') : t('common.inactive')}
              </button>
            ))}
          </div>
        </div>
      </Card>
 
      {/* Table */}
      <Card>
        {isLoading ? (
          <Spinner />
        ) : !providers?.length ? (
          <EmptyState
            icon={<Building2 className="h-12 w-12" />}
            title="No providers found"
            description="Register a supplier to start managing your supply chain."
            action={<Button onClick={() => setShowRegister(true)}>Register Provider</Button>}
          />
        ) : (
          <div className="overflow-x-auto -mx-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Company</th>
                  <th className="hidden md:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Contact</th>
                  <th className="hidden lg:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Email</th>
                  <th className="px-6 pb-3 text-center font-medium text-[var(--text-secondary)]">Products</th>
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