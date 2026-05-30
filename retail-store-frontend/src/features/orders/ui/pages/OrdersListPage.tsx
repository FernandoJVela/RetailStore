import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ShoppingCart, Plus, CheckCircle, Clock, XCircle, DollarSign } from 'lucide-react';
import { Button, Card, Spinner, EmptyState, StatCard, FilterPillBar, PageHeader } from '@shared/components/ui';
import { useOrders } from '@features/orders';
import { OrderRow } from '@features/orders';
import { CreateOrderModal } from '@features/orders';
import { OrderDetailPanel } from '@features/orders';
import type { OrderStatus } from '@features/orders';

type FilterStatus = 'all' | OrderStatus;

export function OrdersListPage() {
  const { t } = useTranslation();
  const [statusFilter, setStatusFilter] = useState<FilterStatus>('all');
  const [showCreate, setShowCreate] = useState(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const filters: { key: FilterStatus; label: string }[] = [
    { key: 'all', label: t('orders.status_all') },
    { key: 'Draft', label: t('orders.status_draft') },
    { key: 'Pending', label: t('orders.status_pending') },
    { key: 'Confirmed', label: t('orders.status_confirmed') },
    { key: 'Shipped', label: t('orders.status_shipped') },
    { key: 'Delivered', label: t('orders.status_delivered') },
    { key: 'Completed', label: t('orders.status_completed') },
    { key: 'Cancelled', label: t('orders.status_cancelled') },
  ];

  const { data: orders, isLoading } = useOrders({
    status: statusFilter === 'all' ? undefined : statusFilter,
  });

  const total = orders?.length ?? 0;
  const activeCount = orders?.filter((o) => ['Confirmed', 'Shipped', 'Delivered'].includes(o.status)).length ?? 0;
  const pendingCount = orders?.filter((o) => o.status === 'Draft' || o.status === 'Pending').length ?? 0;
  const totalRevenue = orders?.filter((o) => !['Cancelled', 'Draft'].includes(o.status)).reduce((s, o) => s + o.totalAmount, 0) ?? 0;

  const summaryCards = [
    { labelKey: 'orders.totalOrders', value: total.toString(), icon: ShoppingCart, iconColor: 'text-primary-600 dark:text-primary-400', iconBg: 'bg-primary-100 dark:bg-primary-500/15' },
    { labelKey: 'common.active', value: activeCount.toString(), icon: CheckCircle, iconColor: 'text-emerald-600 dark:text-emerald-400', iconBg: 'bg-emerald-100 dark:bg-emerald-500/15' },
    { labelKey: 'orders.status_pending', value: pendingCount.toString(), icon: Clock, iconColor: 'text-amber-600 dark:text-amber-400', iconBg: 'bg-amber-100 dark:bg-amber-500/15' },
    { labelKey: 'orders.revenue', value: `$${totalRevenue.toLocaleString('en-US', { minimumFractionDigits: 2 })}`, icon: DollarSign, iconColor: 'text-emerald-600 dark:text-emerald-400', iconBg: 'bg-emerald-100 dark:bg-emerald-500/15' },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('nav.orders')}
        subtitle={t('orders.subtitle')}
        action={
          <Button onClick={() => setShowCreate(true)}>
            <Plus className="h-4 w-4" />
            <span className="hidden sm:inline">{t('orders.createOrder')}</span>
            <span className="sm:hidden">{t('common.new')}</span>
          </Button>
        }
      />

      {/* Summary */}
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

      {/* Status filters */}
      <Card>
        <FilterPillBar
          options={filters}
          value={statusFilter}
          onChange={(v) => setStatusFilter(v as typeof statusFilter)}
        />
      </Card>

      {/* Table */}
      <Card>
        {isLoading ? (
          <Spinner />
        ) : !orders?.length ? (
          <EmptyState
            icon={<ShoppingCart className="h-12 w-12" />}
            title={t('orders.noOrdersFound')}
            description={t('orders.noOrdersDesc')}
            action={<Button onClick={() => setShowCreate(true)}>{t('orders.createOrder')}</Button>}
          />
        ) : (
          <div className="overflow-x-auto -mx-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('orders.col_order')}</th>
                  <th className="hidden sm:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('orders.col_date')}</th>
                  <th className="px-6 pb-3 text-center font-medium text-[var(--text-secondary)]">{t('orders.col_items')}</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('orders.col_total')}</th>
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('common.status')}</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('common.actions')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--border-color)]">
                {orders.map((order) => (
                  <OrderRow key={order.id} order={order} onViewDetail={() => setSelectedId(order.id)} />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      <CreateOrderModal isOpen={showCreate} onClose={() => setShowCreate(false)} />
      {selectedId && (
        <OrderDetailPanel orderId={selectedId} isOpen={!!selectedId} onClose={() => setSelectedId(null)} />
      )}
    </div>
  );
}
