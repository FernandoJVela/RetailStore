import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import {
  ShoppingCart, Package, AlertTriangle, CreditCard,
  Users, ArrowRight, CheckCircle,
} from 'lucide-react';
import { Card, Badge, Spinner, Button } from '@shared/components/ui';
import { cn } from '@shared/lib/utils';
import { formatDate } from '@shared/lib/utils';
import { useAuthStore } from '@shared/store/auth-store';
 
// Cross-module imports — reusing existing hooks
import { useDashboardKpis, useSalesByCategory, useRevenueByPeriod } from '@features/reports/application/hooks/useReportsQueries';
import { useOrders } from '@features/orders/application/hooks/useOrdersQueries';
import { useLowStock } from '@features/inventory/application/hooks/useInventoryQueries';
import { useUnreadCount } from '@features/notifications/application/hooks/useNotificationsQueries';
import { stockStatusVariant } from '@features/inventory';
import { orderStatusVariant } from '@features/orders';
 
// ─── Chart colors ───────────────────────────────────────────
const BAR_COLORS = ['bg-primary-500', 'bg-emerald-500', 'bg-amber-500', 'bg-violet-500', 'bg-rose-500', 'bg-cyan-500'];
 
export function DashboardPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const recipientId = user?.userId ?? '';
 
  const { data: kpis, isLoading: kpisLoading } = useDashboardKpis();
  const { data: categories } = useSalesByCategory();
  const { data: revenue } = useRevenueByPeriod(6);
  const { data: recentOrders } = useOrders();
  const { data: lowStockItems } = useLowStock();
  const { data: unreadCount } = useUnreadCount(recipientId);
 
  // Greeting based on time of day
  const hour = new Date().getHours();
  const greeting = hour < 12 ? 'Good morning' : hour < 18 ? 'Good afternoon' : 'Good evening';
 
  return (
    <div className="space-y-6">
      {/* ─── Welcome Header ──────────────────────────────── */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-[var(--text-primary)]">
            {greeting}, {user?.username ?? 'User'} 👋
          </h1>
          <p className="mt-1 text-sm text-[var(--text-secondary)]">
            Here's what's happening with your store today
          </p>
        </div>
        {!!unreadCount && unreadCount > 0 && (
          <Button variant="outline" size="sm" onClick={() => navigate('/notifications')}>
            <span className="flex h-5 w-5 items-center justify-center rounded-full bg-red-500 text-white text-xs font-bold">{unreadCount}</span>
            Notifications
          </Button>
        )}
      </div>
 
      {/* ─── KPI Cards ───────────────────────────────────── */}
      {kpisLoading ? (
        <Spinner />
      ) : kpis ? (
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          <KpiCard
            label="Today's Revenue"
            value={`$${kpis.todayRevenue.toLocaleString('en-US', { minimumFractionDigits: 0 })}`}
            sub={`${kpis.todayOrders} orders today`}
            icon={CreditCard}
            color="text-emerald-600 dark:text-emerald-400"
            bg="bg-emerald-100 dark:bg-emerald-500/15"
            onClick={() => navigate('/reports')}
          />
          <KpiCard
            label="Month Revenue"
            value={`$${kpis.monthRevenue.toLocaleString('en-US', { minimumFractionDigits: 0 })}`}
            sub={`${kpis.monthOrders} orders this month`}
            icon={ShoppingCart}
            color="text-primary-600 dark:text-primary-400"
            bg="bg-primary-100 dark:bg-primary-500/15"
            onClick={() => navigate('/orders')}
          />
          <KpiCard
            label="Active Catalog"
            value={kpis.activeProducts.toString()}
            sub={`${kpis.activeCustomers} customers`}
            icon={Package}
            color="text-blue-600 dark:text-blue-400"
            bg="bg-blue-100 dark:bg-blue-500/15"
            onClick={() => navigate('/products')}
          />
          <KpiCard
            label="Stock Alerts"
            value={(kpis.outOfStockProducts + kpis.lowStockProducts).toString()}
            sub={`${kpis.outOfStockProducts} out · ${kpis.lowStockProducts} low`}
            icon={AlertTriangle}
            color={kpis.outOfStockProducts > 0 ? 'text-red-600 dark:text-red-400' : 'text-amber-600 dark:text-amber-400'}
            bg={kpis.outOfStockProducts > 0 ? 'bg-red-100 dark:bg-red-500/15' : 'bg-amber-100 dark:bg-amber-500/15'}
            onClick={() => navigate('/inventory')}
          />
        </div>
      ) : null}
 
      {/* ─── Row: Revenue Trend + Sales by Category ──────── */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Mini revenue chart */}
        <Card title="Revenue Trend" subtitle="Last 6 months" actions={
          <button onClick={() => navigate('/reports')} className="text-xs text-primary-600 hover:text-primary-500 flex items-center gap-1">
            View all <ArrowRight className="h-3 w-3" />
          </button>
        }>
          {revenue?.length ? (
            <div className="flex items-end gap-3 h-32">
              {revenue.map((item) => {
                const max = Math.max(...revenue.map((r) => r.revenue));
                const pct = max > 0 ? (item.revenue / max) * 100 : 0;
                return (
                  <div key={item.period} className="flex-1 flex flex-col items-center justify-end group relative">
                    <div className="hidden group-hover:block absolute -top-12 rounded-lg bg-[var(--sidebar-bg)] text-white px-2.5 py-1.5 text-xs shadow-lg z-10 whitespace-nowrap">
                      {item.formattedRevenue} · {item.orderCount} orders
                    </div>
                    <div
                      className="w-full rounded-t-md bg-primary-500 hover:bg-primary-400 transition-all cursor-pointer"
                      style={{ height: `${Math.max(pct, 4)}%` }}
                    />
                    <span className="text-[10px] text-[var(--text-muted)] mt-1.5 tabular-nums">{item.period.split('-')[1]}</span>
                  </div>
                );
              })}
            </div>
          ) : (
            <p className="text-sm text-[var(--text-muted)] italic py-8 text-center">No revenue data yet</p>
          )}
        </Card>
 
        {/* Category breakdown */}
        <Card title="Sales by Category" actions={
          <button onClick={() => navigate('/reports')} className="text-xs text-primary-600 hover:text-primary-500 flex items-center gap-1">
            View all <ArrowRight className="h-3 w-3" />
          </button>
        }>
          {categories?.length ? (
            <div className="space-y-3">
              {categories.slice(0, 4).map((cat, idx) => {
                const max = Math.max(...categories.map((c) => c.totalRevenue));
                return (
                  <div key={cat.category}>
                    <div className="flex items-center justify-between mb-1">
                      <span className="text-sm font-medium text-[var(--text-primary)]">{cat.category}</span>
                      <span className="text-sm font-semibold text-[var(--text-primary)] tabular-nums">{cat.formattedRevenue}</span>
                    </div>
                    <div className="h-2 rounded-full bg-[var(--bg-tertiary)] overflow-hidden">
                      <div className={cn('h-full rounded-full', BAR_COLORS[idx % BAR_COLORS.length])}
                        style={{ width: `${(cat.totalRevenue / max) * 100}%` }} />
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <p className="text-sm text-[var(--text-muted)] italic py-8 text-center">No sales data yet</p>
          )}
        </Card>
      </div>
 
      {/* ─── Row: Recent Orders + Low Stock Alerts ───────── */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Recent orders */}
        <Card title="Recent Orders" actions={
          <button onClick={() => navigate('/orders')} className="text-xs text-primary-600 hover:text-primary-500 flex items-center gap-1">
            View all <ArrowRight className="h-3 w-3" />
          </button>
        }>
          {recentOrders?.length ? (
            <div className="space-y-2">
              {recentOrders.slice(0, 5).map((order) => (
                <button
                  key={order.id}
                  onClick={() => navigate('/orders')}
                  className="flex w-full items-center justify-between rounded-lg bg-[var(--bg-primary)] border border-[var(--border-color)] px-4 py-3 hover:bg-[var(--bg-tertiary)]/50 transition-colors text-left"
                >
                  <div className="min-w-0 flex-1">
                    <div className="flex items-center gap-2">
                      <span className="text-sm font-mono text-[var(--text-primary)]">#{order.id.substring(0, 8)}</span>
                      <Badge variant={orderStatusVariant(order.status)}>{order.status}</Badge>
                    </div>
                    <p className="text-xs text-[var(--text-muted)] mt-0.5">{formatDate(order.orderDate)} · {order.itemCount} items</p>
                  </div>
                  <span className="text-sm font-semibold text-[var(--text-primary)] tabular-nums shrink-0 ml-4">{order.formattedTotal}</span>
                </button>
              ))}
            </div>
          ) : (
            <p className="text-sm text-[var(--text-muted)] italic py-8 text-center">No orders yet</p>
          )}
        </Card>
 
        {/* Low stock alerts */}
        <Card title="Stock Alerts" subtitle={lowStockItems?.length ? `${lowStockItems.length} items need attention` : undefined} actions={
          <button onClick={() => navigate('/inventory')} className="text-xs text-primary-600 hover:text-primary-500 flex items-center gap-1">
            View all <ArrowRight className="h-3 w-3" />
          </button>
        }>
          {lowStockItems?.length ? (
            <div className="space-y-2">
              {lowStockItems.slice(0, 5).map((item) => (
                <div
                  key={item.id}
                  className={cn(
                    'flex items-center justify-between rounded-lg border px-4 py-3',
                    item.isOutOfStock
                      ? 'border-red-200 bg-red-50/50 dark:border-red-800 dark:bg-red-500/5'
                      : 'border-amber-200 bg-amber-50/50 dark:border-amber-800 dark:bg-amber-500/5'
                  )}
                >
                  <div className="min-w-0">
                    <p className="text-sm font-medium text-[var(--text-primary)] truncate">{item.productName}</p>
                    <p className="text-xs text-[var(--text-muted)] font-mono">{item.sku}</p>
                  </div>
                  <div className="flex items-center gap-3 shrink-0 ml-4">
                    <span className="text-sm font-semibold tabular-nums text-[var(--text-primary)]">{item.availableQuantity}</span>
                    <Badge variant={stockStatusVariant(item.stockStatus)}>
                      {item.isOutOfStock ? 'Out' : 'Low'}
                    </Badge>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="flex flex-col items-center py-8">
              <CheckCircle className="h-10 w-10 text-emerald-500 mb-2" />
              <p className="text-sm text-[var(--text-secondary)]">All stock levels healthy</p>
            </div>
          )}
        </Card>
      </div>
 
      {/* ─── Quick Actions ───────────────────────────────── */}
      <Card title="Quick Actions">
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
          {[
            { label: 'New Order', icon: ShoppingCart, path: '/orders', color: 'text-blue-600 dark:text-blue-400', bg: 'bg-blue-100 dark:bg-blue-500/15' },
            { label: 'Add Product', icon: Package, path: '/products', color: 'text-emerald-600 dark:text-emerald-400', bg: 'bg-emerald-100 dark:bg-emerald-500/15' },
            { label: 'View Reports', icon: CreditCard, path: '/reports', color: 'text-violet-600 dark:text-violet-400', bg: 'bg-violet-100 dark:bg-violet-500/15' },
            { label: 'Manage Users', icon: Users, path: '/users', color: 'text-amber-600 dark:text-amber-400', bg: 'bg-amber-100 dark:bg-amber-500/15' },
          ].map(({ label, icon: Icon, path, color, bg }) => (
            <button
              key={label}
              onClick={() => navigate(path)}
              className="flex flex-col items-center gap-2 rounded-xl border border-[var(--border-color)] bg-[var(--bg-primary)] p-4 hover:bg-[var(--bg-tertiary)] hover:border-primary-300 dark:hover:border-primary-700 transition-all"
            >
              <div className={cn('flex h-10 w-10 items-center justify-center rounded-lg', bg)}>
                <Icon className={cn('h-5 w-5', color)} />
              </div>
              <span className="text-sm font-medium text-[var(--text-primary)]">{label}</span>
            </button>
          ))}
        </div>
      </Card>
    </div>
  );
}
 
// ─── KPI Card Sub-component ─────────────────────────────────
function KpiCard({ label, value, sub, icon: Icon, color, bg, onClick }: {
  label: string; value: string; sub: string;
  icon: typeof CreditCard; color: string; bg: string;
  onClick: () => void;
}) {
  return (
    <button
      onClick={onClick}
      className="rounded-xl border border-[var(--border-color)] bg-[var(--bg-secondary)] p-4 text-left hover:border-primary-300 dark:hover:border-primary-700 hover:shadow-sm transition-all"
    >
      <div className="flex items-center justify-between mb-2">
        <p className="text-xs text-[var(--text-secondary)] font-medium">{label}</p>
        <div className={cn('flex h-8 w-8 items-center justify-center rounded-lg', bg)}>
          <Icon className={cn('h-4 w-4', color)} />
        </div>
      </div>
      <p className="text-2xl font-bold text-[var(--text-primary)] tabular-nums">{value}</p>
      <p className="text-xs text-[var(--text-muted)] mt-1">{sub}</p>
    </button>
  );
}