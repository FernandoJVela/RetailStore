import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ShoppingCart, Plus, CheckCircle, Clock, XCircle, DollarSign } from 'lucide-react';
import { Button, Card, Spinner, EmptyState } from '@shared/components/ui';
import { useOrders } from '@features/orders';
import { OrderRow } from '@features/orders';
import { CreateOrderModal } from '@features/orders';
import { OrderDetailPanel } from '@features/orders';
import type { OrderStatus } from '@features/orders';
 
type FilterStatus = 'all' | OrderStatus;
 
const FILTERS: { key: FilterStatus; label: string }[] = [
  { key: 'all', label: 'All' },
  { key: 'Draft', label: 'Draft' },
  { key: 'Pending', label: 'Pending' },
  { key: 'Confirmed', label: 'Confirmed' },
  { key: 'Shipped', label: 'Shipped' },
  { key: 'Delivered', label: 'Delivered' },
  { key: 'Completed', label: 'Completed' },
  { key: 'Cancelled', label: 'Cancelled' },
];
 
export function OrdersListPage() {
  const { t } = useTranslation();
  const [statusFilter, setStatusFilter] = useState<FilterStatus>('all');
  const [showCreate, setShowCreate] = useState(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);
 
  const { data: orders, isLoading } = useOrders({
    status: statusFilter === 'all' ? undefined : statusFilter,
  });
 
  const total = orders?.length ?? 0;
  const activeCount = orders?.filter((o) => ['Confirmed', 'Shipped', 'Delivered'].includes(o.status)).length ?? 0;
  const pendingCount = orders?.filter((o) => o.status === 'Draft' || o.status === 'Pending').length ?? 0;
  const totalRevenue = orders?.filter((o) => !['Cancelled', 'Draft'].includes(o.status)).reduce((s, o) => s + o.totalAmount, 0) ?? 0;
 
  const summaryCards = [
    { label: 'Total Orders', value: total.toString(), icon: ShoppingCart, color: 'text-primary-600 dark:text-primary-400', bg: 'bg-primary-100 dark:bg-primary-500/15' },
    { label: 'Active', value: activeCount.toString(), icon: CheckCircle, color: 'text-emerald-600 dark:text-emerald-400', bg: 'bg-emerald-100 dark:bg-emerald-500/15' },
    { label: 'Pending', value: pendingCount.toString(), icon: Clock, color: 'text-amber-600 dark:text-amber-400', bg: 'bg-amber-100 dark:bg-amber-500/15' },
    { label: 'Revenue', value: `$${totalRevenue.toLocaleString('en-US', { minimumFractionDigits: 2 })}`, icon: DollarSign, color: 'text-emerald-600 dark:text-emerald-400', bg: 'bg-emerald-100 dark:bg-emerald-500/15' },
  ];
 
  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-[var(--text-primary)]">{t('nav.orders')}</h1>
          <p className="mt-1 text-sm text-[var(--text-secondary)]">Manage customer orders and track their lifecycle</p>
        </div>
        <Button onClick={() => setShowCreate(true)}>
          <Plus className="h-4 w-4" />
          <span className="hidden sm:inline">Create Order</span>
          <span className="sm:hidden">New</span>
        </Button>
      </div>
 
      {/* Summary */}
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
 
      {/* Status filters */}
      <Card>
        <div className="flex flex-wrap gap-2">
          {FILTERS.map(({ key, label }) => (
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
      </Card>
 
      {/* Table */}
      <Card>
        {isLoading ? (
          <Spinner />
        ) : !orders?.length ? (
          <EmptyState
            icon={<ShoppingCart className="h-12 w-12" />}
            title="No orders found"
            description="Create an order to get started."
            action={<Button onClick={() => setShowCreate(true)}>Create Order</Button>}
          />
        ) : (
          <div className="overflow-x-auto -mx-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Order</th>
                  <th className="hidden sm:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Date</th>
                  <th className="px-6 pb-3 text-center font-medium text-[var(--text-secondary)]">Items</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">Total</th>
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