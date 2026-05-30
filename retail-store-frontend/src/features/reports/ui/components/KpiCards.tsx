import { useTranslation } from 'react-i18next';
import {
  ShoppingCart, DollarSign, Package, AlertTriangle,
  Truck, TrendingUp,
} from 'lucide-react';
import { Spinner } from '@shared/components/ui';
import { useDashboardKpis } from '@features/reports/application/hooks/useReportsQueries';

export function KpiCards() {
  const { t } = useTranslation();
  const { data: kpis, isLoading } = useDashboardKpis();

  if (isLoading) return <Spinner />;
  if (!kpis) return null;

  const cards = [
    { labelKey: 'reports.todayOrders', value: kpis.todayOrders.toString(), sub: t('reports.thisWeek', { count: kpis.weekOrders }), icon: ShoppingCart, color: 'text-primary-600 dark:text-primary-400', bg: 'bg-primary-100 dark:bg-primary-500/15' },
    { labelKey: 'reports.todayRevenue', value: kpis.formattedTodayRevenue, sub: t('reports.thisMonth', { count: kpis.formattedMonthRevenue }), icon: DollarSign, color: 'text-emerald-600 dark:text-emerald-400', bg: 'bg-emerald-100 dark:bg-emerald-500/15' },
    { labelKey: 'reports.activeProducts', value: kpis.activeProducts.toString(), sub: t('reports.customerCount', { count: kpis.activeCustomers }), icon: Package, color: 'text-blue-600 dark:text-blue-400', bg: 'bg-blue-100 dark:bg-blue-500/15' },
    { labelKey: 'reports.monthOrders', value: kpis.monthOrders.toString(), sub: t('reports.revenueValue', { value: kpis.formattedMonthRevenue }), icon: TrendingUp, color: 'text-violet-600 dark:text-violet-400', bg: 'bg-violet-100 dark:bg-violet-500/15' },
    { labelKey: 'reports.pendingShipments', value: kpis.pendingShipments.toString(), sub: t('reports.pendingPayments', { count: kpis.pendingPayments }), icon: Truck, color: 'text-amber-600 dark:text-amber-400', bg: 'bg-amber-100 dark:bg-amber-500/15' },
    { labelKey: 'reports.stockAlerts', value: (kpis.outOfStockProducts + kpis.lowStockProducts).toString(), sub: t('reports.stockSub', { out: kpis.outOfStockProducts, low: kpis.lowStockProducts }), icon: AlertTriangle, color: 'text-red-600 dark:text-red-400', bg: 'bg-red-100 dark:bg-red-500/15' },
  ];

  return (
    <div className="grid grid-cols-2 gap-4 lg:grid-cols-3 xl:grid-cols-6">
      {cards.map(({ labelKey, value, sub, icon: Icon, color, bg }) => (
        <div key={labelKey} className="rounded-xl border border-[var(--border-color)] bg-[var(--bg-secondary)] p-4">
          <div className="flex items-center justify-between mb-2">
            <div className={`flex h-9 w-9 items-center justify-center rounded-lg ${bg}`}>
              <Icon className={`h-4 w-4 ${color}`} />
            </div>
          </div>
          <p className="text-2xl font-bold text-[var(--text-primary)] tabular-nums">{value}</p>
          <p className="text-xs text-[var(--text-muted)] mt-1">{sub}</p>
          <p className="text-xs text-[var(--text-secondary)] mt-0.5">{t(labelKey)}</p>
        </div>
      ))}
    </div>
  );
}
