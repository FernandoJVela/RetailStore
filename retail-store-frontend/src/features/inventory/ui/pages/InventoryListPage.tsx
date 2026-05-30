import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Warehouse, AlertTriangle, PackageX, PackageCheck } from 'lucide-react';
import { Card, Spinner, EmptyState, StatCard, SearchInput, PageHeader } from '@shared/components/ui';
import { useDebounce } from '@shared/hooks';
import { useInventory } from '@features/inventory/application/hooks/useInventoryQueries';
import { InventoryRow } from '@features/inventory/ui/components/InventoryRow';
import { StockActionModal } from '@features/inventory/ui/components/StockActionModal';
import type { InventoryItem, StockStatus } from '@features/inventory';
 
type FilterStatus = 'all' | StockStatus;
 
export function InventoryListPage() {
  const { t } = useTranslation();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<FilterStatus>('all');
  const [actionTarget, setActionTarget] = useState<{ item: InventoryItem; action: 'add' | 'remove' | 'adjust' | 'threshold' } | null>(null);
 
  const debouncedSearch = useDebounce(search, 300);
  const { data: items, isLoading } = useInventory({
    stockStatus: statusFilter === 'all' ? undefined : statusFilter,
  });
 
  const filtered = items?.filter((item) => {
    if (!debouncedSearch) return true;
    const s = debouncedSearch.toLowerCase();
    return item.productName.toLowerCase().includes(s) || item.sku.toLowerCase().includes(s);
  });
 
  // Summary stats
  const totalItems = items?.length ?? 0;
  const inStockCount = items?.filter((i) => i.stockStatus === 'InStock').length ?? 0;
  const lowStockCount = items?.filter((i) => i.stockStatus === 'LowStock').length ?? 0;
  const outOfStockCount = items?.filter((i) => i.stockStatus === 'OutOfStock').length ?? 0;
 
  const summaryCards = [
    { labelKey: 'inventory.totalItems', value: totalItems, icon: Warehouse, iconColor: 'text-primary-600 dark:text-primary-400', iconBg: 'bg-primary-100 dark:bg-primary-500/15' },
    { labelKey: 'inventory.inStock', value: inStockCount, icon: PackageCheck, iconColor: 'text-emerald-600 dark:text-emerald-400', iconBg: 'bg-emerald-100 dark:bg-emerald-500/15' },
    { labelKey: 'inventory.lowStock', value: lowStockCount, icon: AlertTriangle, iconColor: 'text-amber-600 dark:text-amber-400', iconBg: 'bg-amber-100 dark:bg-amber-500/15' },
    { labelKey: 'inventory.outOfStock', value: outOfStockCount, icon: PackageX, iconColor: 'text-red-600 dark:text-red-400', iconBg: 'bg-red-100 dark:bg-red-500/15' },
  ];
 
  return (
    <div className="space-y-6">
      {/* Header */}
      <PageHeader title={t('nav.inventory')} subtitle={t('inventory.subtitle')} />
 
      {/* Summary cards */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        {summaryCards.map(({ labelKey, value, icon, iconColor, iconBg }) => (
          <StatCard
            key={labelKey}
            label={t(labelKey)}
            value={value}
            icon={icon}
            iconColor={iconColor}
            iconBg={iconBg}
          />
        ))}
      </div>
 
      {/* Filters */}
      <Card>
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
          <SearchInput
            value={search}
            onChange={setSearch}
            placeholder={t('inventory.searchPlaceholder')}
          />
          <div className="flex flex-wrap gap-2">
            {([
              { key: 'all' as FilterStatus, label: t('common.all') },
              { key: 'InStock' as FilterStatus, label: t('inventory.inStock') },
              { key: 'LowStock' as FilterStatus, label: t('inventory.lowStock') },
              { key: 'OutOfStock' as FilterStatus, label: t('inventory.outOfStock') },
            ]).map(({ key, label }) => (
              <button
                key={key}
                onClick={() => setStatusFilter(key)}
                className={`rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
                  statusFilter === key
                    ? 'bg-primary-600 text-white'
                    : 'bg-[var(--bg-primary)] text-[var(--text-secondary)] hover:bg-[var(--bg-tertiary)]'
                }`}
              >
                {label}
              </button>
            ))}
          </div>
        </div>
      </Card>
 
      {/* Table */}
      <Card>
        {isLoading ? (
          <Spinner />
        ) : !filtered?.length ? (
          <EmptyState
            icon={<Warehouse className="h-12 w-12" />}
            title={t('inventory.noInventoryFound')}
            description={t('inventory.noInventoryDesc')}
          />
        ) : (
          <div className="overflow-x-auto -mx-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('inventory.col_product')}</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('inventory.col_onHand')}</th>
                  <th className="hidden md:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('inventory.col_reserved')}</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('inventory.col_available')}</th>
                  <th className="hidden lg:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('inventory.col_threshold')}</th>
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('common.status')}</th>
                  <th className="hidden sm:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('inventory.col_health')}</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('common.actions')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--border-color)]">
                {filtered.map((item) => (
                  <InventoryRow
                    key={item.id}
                    item={item}
                    onAction={(action) => setActionTarget({ item, action })}
                  />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
 
      {/* Stock action modal */}
      {actionTarget && (
        <StockActionModal
          item={actionTarget.item}
          action={actionTarget.action}
          isOpen={!!actionTarget}
          onClose={() => setActionTarget(null)}
        />
      )}
    </div>
  );
}