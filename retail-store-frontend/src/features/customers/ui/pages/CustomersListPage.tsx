import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { UserPlus, Users } from 'lucide-react';
import { Button, Card, Spinner, EmptyState, SearchInput, FilterPillBar, PageHeader } from '@shared/components/ui';
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
      <PageHeader
        title={t('nav.customers')}
        subtitle={t('customers.subtitle', { count: customers?.length ?? 0 })}
        action={
          <Button onClick={() => setShowRegister(true)}>
            <UserPlus className="h-4 w-4" />
            <span className="hidden sm:inline">{t('customers.registerCustomer')}</span>
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
            placeholder={t('customers.searchPlaceholder')}
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
        ) : !customers?.length ? (
          <EmptyState
            icon={<Users className="h-12 w-12" />}
            title={t('customers.noCustomersFound')}
            description={t('customers.noCustomersDesc')}
            action={<Button onClick={() => setShowRegister(true)}>{t('customers.registerCustomer')}</Button>}
          />
        ) : (
          <div className="overflow-x-auto -mx-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('customers.col_customer')}</th>
                  <th className="hidden md:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('users.email')}</th>
                  <th className="hidden lg:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('customers.col_phone')}</th>
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('common.status')}</th>
                  <th className="hidden sm:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('customers.col_registered')}</th>
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