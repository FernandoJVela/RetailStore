import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, UserPlus, Users } from 'lucide-react';
import { Button, Card, Spinner, EmptyState } from '@shared/components/ui';
import { useDebounce } from '@shared/hooks';
import { useCustomers } from '@features/customers/application/hooks/useCustomersQueries';
import { CustomerRow } from '@features/customers/ui/components/CustomerRow';
import { RegisterCustomerModal } from '@features/customers/ui/components/RegisterCustomerModal';
import { CustomerDetailPanel } from '@features/customers/ui/components/CustomerDetailPanel';
 
export function CustomersListPage() {
  const { t } = useTranslation();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'inactive'>('all');
  const [showRegister, setShowRegister] = useState(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);
 
  const debouncedSearch = useDebounce(search, 300);
  const { data: customers, isLoading } = useCustomers({
    search: debouncedSearch || undefined,
    isActive: statusFilter === 'all' ? undefined : statusFilter === 'active',
  });
 
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-[var(--text-primary)]">{t('nav.customers')}</h1>
          <p className="mt-1 text-sm text-[var(--text-secondary)]">
            {customers?.length ?? 0} {t('nav.customers').toLowerCase()} registered
          </p>
        </div>
        <Button onClick={() => setShowRegister(true)}>
          <UserPlus className="h-4 w-4" />
          <span className="hidden sm:inline">Register Customer</span>
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
              placeholder="Search by name or email..."
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
        ) : !customers?.length ? (
          <EmptyState
            icon={<Users className="h-12 w-12" />}
            title="No customers found"
            description="Try adjusting your search or register a new customer."
            action={<Button onClick={() => setShowRegister(true)}>Register Customer</Button>}
          />
        ) : (
          <div className="overflow-x-auto -mx-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Customer</th>
                  <th className="hidden md:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Email</th>
                  <th className="hidden lg:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Phone</th>
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('common.status')}</th>
                  <th className="hidden sm:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Registered</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('common.actions')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--border-color)]">
                {customers.map((customer) => (
                  <CustomerRow
                    key={customer.id}
                    customer={customer}
                    onViewDetail={() => setSelectedId(customer.id)}
                  />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
 
      {/* Register modal */}
      <RegisterCustomerModal
        isOpen={showRegister}
        onClose={() => setShowRegister(false)}
      />
 
      {/* Detail panel */}
      {selectedId && (
        <CustomerDetailPanel
          customerId={selectedId}
          isOpen={!!selectedId}
          onClose={() => setSelectedId(null)}
        />
      )}
    </div>
  );
}