import { useTranslation } from 'react-i18next';
import { Card, Badge, Spinner } from '@shared/components/ui';
import { cn } from '@shared/lib/utils';
import { usePaymentAnalytics, useInventoryHealth, useShippingPerformance } from '@features/reports/application/hooks/useReportsQueries';

// ═══════════════════════════════════════════════════════════
// PAYMENT METHODS BREAKDOWN
// ═══════════════════════════════════════════════════════════
export function PaymentMethodsPanel() {
  const { t } = useTranslation();
  const { data, isLoading } = usePaymentAnalytics();

  if (isLoading) return <Card title={t('reports.paymentMethods')}><Spinner /></Card>;
  if (!data?.length) return null;

  const totalPayments = data.reduce((s, d) => s + d.paymentCount, 0);

  return (
    <Card title={t('reports.paymentMethods')} subtitle={t('reports.paymentMethodsSubtitle')}>
      <div className="space-y-4">
        {data.map((method) => {
          const sharePercent = totalPayments > 0 ? (method.paymentCount / totalPayments) * 100 : 0;
          return (
            <div key={method.method} className="flex items-center gap-4">
              <div className="flex-1 min-w-0">
                <div className="flex items-center justify-between mb-1">
                  <span className="text-sm font-medium text-[var(--text-primary)]">{method.methodLabel}</span>
                  <span className="text-sm text-[var(--text-secondary)] tabular-nums">{method.formattedTotal}</span>
                </div>
                <div className="h-2 w-full rounded-full bg-[var(--bg-tertiary)] overflow-hidden">
                  <div className="h-full rounded-full bg-primary-500 transition-all" style={{ width: `${sharePercent}%` }} />
                </div>
                <div className="flex gap-3 mt-1 text-xs text-[var(--text-muted)]">
                  <span>{t('reports.payments', { count: method.paymentCount })}</span>
                  <span className={method.successRate >= 90 ? 'text-emerald-600 dark:text-emerald-400' : 'text-amber-600 dark:text-amber-400'}>
                    {t('reports.successRate', { rate: method.successRate.toFixed(1) })}
                  </span>
                  {method.failedCount > 0 && <span className="text-red-500">{t('reports.failedCount', { count: method.failedCount })}</span>}
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </Card>
  );
}

// ═══════════════════════════════════════════════════════════
// INVENTORY HEALTH BY CATEGORY
// ═══════════════════════════════════════════════════════════
export function InventoryHealthPanel() {
  const { t } = useTranslation();
  const { data, isLoading } = useInventoryHealth();

  if (isLoading) return <Card title={t('reports.inventoryHealth')}><Spinner /></Card>;
  if (!data?.length) return null;

  return (
    <Card title={t('reports.inventoryHealth')} subtitle={t('reports.inventoryHealthSubtitle')}>
      <div className="space-y-4">
        {data.map((cat) => (
          <div key={cat.category} className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-4">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-semibold text-[var(--text-primary)]">{cat.category}</span>
              <span className={cn(
                'text-sm font-bold tabular-nums',
                cat.healthPercent >= 80 ? 'text-emerald-600 dark:text-emerald-400'
                  : cat.healthPercent >= 50 ? 'text-amber-600 dark:text-amber-400'
                  : 'text-red-600 dark:text-red-400'
              )}>
                {cat.healthPercent}%
              </span>
            </div>
            <div className="h-2 w-full rounded-full bg-[var(--bg-tertiary)] overflow-hidden mb-2">
              <div
                className={cn('h-full rounded-full transition-all',
                  cat.healthPercent >= 80 ? 'bg-emerald-500'
                    : cat.healthPercent >= 50 ? 'bg-amber-500' : 'bg-red-500'
                )}
                style={{ width: `${cat.healthPercent}%` }}
              />
            </div>
            <div className="flex flex-wrap gap-3 text-xs text-[var(--text-muted)]">
              <span>{t('reports.productCount', { count: cat.productCount })}</span>
              <span>{t('reports.availableCount', { count: cat.totalAvailable.toLocaleString() })}</span>
              {cat.outOfStockCount > 0 && <Badge variant="danger">{t('reports.outOfStockCount', { count: cat.outOfStockCount })}</Badge>}
              {cat.lowStockCount > 0 && <Badge variant="warning">{t('reports.lowCount', { count: cat.lowStockCount })}</Badge>}
            </div>
          </div>
        ))}
      </div>
    </Card>
  );
}

// ═══════════════════════════════════════════════════════════
// SHIPPING PERFORMANCE BY CARRIER
// ═══════════════════════════════════════════════════════════
export function ShippingPerformancePanel() {
  const { t } = useTranslation();
  const { data, isLoading } = useShippingPerformance();

  if (isLoading) return <Card title={t('reports.shippingPerformance')}><Spinner /></Card>;
  if (!data?.length) return null;

  return (
    <Card title={t('reports.shippingPerformance')} subtitle={t('reports.shippingPerformanceSubtitle')}>
      <div className="overflow-x-auto -mx-6">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-[var(--border-color)]">
              <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('reports.col_carrier')}</th>
              <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('reports.col_shipments')}</th>
              <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('reports.col_success')}</th>
              <th className="hidden md:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('reports.col_avgTime')}</th>
              <th className="hidden md:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('reports.col_avgCost')}</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-[var(--border-color)]">
            {data.map((carrier) => (
              <tr key={carrier.carrier} className="hover:bg-[var(--bg-tertiary)]/50 transition-colors">
                <td className="px-6 py-3 font-medium text-[var(--text-primary)]">{carrier.carrier}</td>
                <td className="px-6 py-3 text-right tabular-nums text-[var(--text-primary)]">
                  {carrier.totalShipments}
                </td>
                <td className="px-6 py-3 text-right">
                  <span className={cn(
                    'font-semibold tabular-nums',
                    carrier.deliverySuccessRate >= 90 ? 'text-emerald-600 dark:text-emerald-400'
                      : carrier.deliverySuccessRate >= 70 ? 'text-amber-600 dark:text-amber-400'
                      : 'text-red-600 dark:text-red-400'
                  )}>
                    {carrier.deliverySuccessRate.toFixed(1)}%
                  </span>
                </td>
                <td className="hidden md:table-cell px-6 py-3 text-right text-[var(--text-secondary)]">
                  {carrier.avgDeliveryDays ?? '—'}
                </td>
                <td className="hidden md:table-cell px-6 py-3 text-right tabular-nums text-[var(--text-secondary)]">
                  {carrier.formattedCost}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Card>
  );
}
