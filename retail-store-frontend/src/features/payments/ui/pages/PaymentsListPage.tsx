import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { CreditCard, CheckCircle, Clock } from 'lucide-react';
import { Card, Spinner, EmptyState, StatCard, FilterPillBar, PageHeader } from '@shared/components/ui';
import { usePayments } from '@features/payments/application/hooks/usePaymentsQueries';
import { PaymentRow } from '@features/payments';
import { PaymentDetailPanel } from '@features/payments';
import type { PaymentStatus } from '@features/payments';

type FilterStatus = 'all' | PaymentStatus;

export function PaymentsListPage() {
  const { t } = useTranslation();
  const [statusFilter, setStatusFilter] = useState<FilterStatus>('all');
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const statusFilters: { key: FilterStatus; label: string }[] = [
    { key: 'all', label: t('payments.status_all') },
    { key: 'Pending', label: t('payments.status_pending') },
    { key: 'Authorized', label: t('payments.status_authorized') },
    { key: 'Captured', label: t('payments.status_captured') },
    { key: 'Failed', label: t('payments.status_failed') },
    { key: 'Refunded', label: t('payments.status_refunded') },
    { key: 'Cancelled', label: t('payments.status_cancelled') },
  ];

  const { data: payments, isLoading } = usePayments({
    status: statusFilter === 'all' ? undefined : statusFilter,
  });

  // Summary stats
  const total = payments?.length ?? 0;
  const capturedCount = payments?.filter((p) => p.status === 'Captured').length ?? 0;
  const pendingCount = payments?.filter((p) => p.status === 'Pending' || p.status === 'Authorized').length ?? 0;
  const totalRevenue = payments?.filter((p) => p.status === 'Captured').reduce((s, p) => s + p.netAmount, 0) ?? 0;

  const summaryCards = [
    { labelKey: 'payments.totalPayments', value: total.toString(), icon: CreditCard, iconColor: 'text-primary-600 dark:text-primary-400', iconBg: 'bg-primary-100 dark:bg-primary-500/15' },
    { labelKey: 'payments.captured', value: capturedCount.toString(), icon: CheckCircle, iconColor: 'text-emerald-600 dark:text-emerald-400', iconBg: 'bg-emerald-100 dark:bg-emerald-500/15' },
    { labelKey: 'orders.status_pending', value: pendingCount.toString(), icon: Clock, iconColor: 'text-amber-600 dark:text-amber-400', iconBg: 'bg-amber-100 dark:bg-amber-500/15' },
    { labelKey: 'payments.revenue', value: `$${totalRevenue.toLocaleString('en-US', { minimumFractionDigits: 2 })}`, icon: CheckCircle, iconColor: 'text-emerald-600 dark:text-emerald-400', iconBg: 'bg-emerald-100 dark:bg-emerald-500/15' },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <PageHeader title={t('nav.payments')} subtitle={t('payments.subtitle')} />

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

      {/* Status filters */}
      <Card>
        <FilterPillBar
          options={statusFilters}
          value={statusFilter}
          onChange={(v) => setStatusFilter(v as typeof statusFilter)}
        />
      </Card>

      {/* Table */}
      <Card>
        {isLoading ? (
          <Spinner />
        ) : !payments?.length ? (
          <EmptyState
            icon={<CreditCard className="h-12 w-12" />}
            title={t('payments.noPaymentsFound')}
            description={t('payments.noPaymentsDesc')}
          />
        ) : (
          <div className="overflow-x-auto -mx-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('payments.col_payment')}</th>
                  <th className="hidden md:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('payments.col_method')}</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('payments.col_amount')}</th>
                  <th className="hidden lg:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('payments.col_net')}</th>
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('common.status')}</th>
                  <th className="hidden sm:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('payments.col_gateway')}</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('common.actions')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--border-color)]">
                {payments.map((payment) => (
                  <PaymentRow
                    key={payment.id}
                    payment={payment}
                    onViewDetail={() => setSelectedId(payment.id)}
                  />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {/* Detail panel */}
      {selectedId && (
        <PaymentDetailPanel
          paymentId={selectedId}
          isOpen={!!selectedId}
          onClose={() => setSelectedId(null)}
        />
      )}
    </div>
  );
}
