import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, Warehouse, AlertTriangle, PackageX, PackageCheck } from 'lucide-react';
import { Card, Spinner, EmptyState } from '@shared/components/ui';
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
    { label: 'Total Items', value: totalItems, icon: Warehouse, color: 'text-primary-600 dark:text-primary-400', bg: 'bg-primary-100 dark:bg-primary-500/15' },
    { label: 'In Stock', value: inStockCount, icon: PackageCheck, color: 'text-emerald-600 dark:text-emerald-400', bg: 'bg-emerald-100 dark:bg-emerald-500/15' },
    { label: 'Low Stock', value: lowStockCount, icon: AlertTriangle, color: 'text-amber-600 dark:text-amber-400', bg: 'bg-amber-100 dark:bg-amber-500/15' },
    { label: 'Out of Stock', value: outOfStockCount, icon: PackageX, color: 'text-red-600 dark:text-red-400', bg: 'bg-red-100 dark:bg-red-500/15' },
  ];
 
  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-[var(--text-primary)]">{t('nav.inventory')}</h1>
        <p className="mt-1 text-sm text-[var(--text-secondary)]">Monitor stock levels, adjust quantities, and manage reorder thresholds</p>
      </div>
 
      {/* Summary cards */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        {summaryCards.map(({ label, value, icon: Icon, color, bg }) => (
          <div key={label} className="rounded-xl border border-[var(--border-color)] bg-[var(--bg-secondary)] p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-[var(--text-secondary)]">{label}</p>
                <p className="mt-1 text-2xl font-bold text-[var(--text-primary)]">{value}</p>
              </div>
              <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${bg}`}>
                <Icon className={`h-5 w-5 ${color}`} />
              </div>
            </div>
          </div>
        ))}
      </div>
 
      {/* Filters */}
      <Card>
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-[var(--text-muted)]" />
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search by product name or SKU..."
              className="w-full rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] py-2.5 pl-10 pr-4 text-sm text-[var(--text-primary)] placeholder:text-[var(--text-muted)] focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div className="flex flex-wrap gap-2">
            {([
              { key: 'all' as FilterStatus, label: 'All' },
              { key: 'InStock' as FilterStatus, label: 'In Stock' },
              { key: 'LowStock' as FilterStatus, label: 'Low Stock' },
              { key: 'OutOfStock' as FilterStatus, label: 'Out of Stock' },
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
            title="No inventory items found"
            description="Adjust filters or create inventory records for your products."
          />
        ) : (
          <div className="overflow-x-auto -mx-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Product</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">On Hand</th>
                  <th className="hidden md:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">Reserved</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">Available</th>
                  <th className="hidden lg:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">Threshold</th>
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('common.status')}</th>
                  <th className="hidden sm:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Health</th>
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