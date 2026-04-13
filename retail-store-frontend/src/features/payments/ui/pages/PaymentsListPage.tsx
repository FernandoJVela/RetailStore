import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { CreditCard, CheckCircle, Clock } from 'lucide-react';
import { Card, Spinner, EmptyState } from '@shared/components/ui';
import { usePayments } from '@features/payments/application/hooks/usePaymentsQueries';
import { PaymentRow } from '@features/payments';
import { PaymentDetailPanel } from '@features/payments';
import type { PaymentStatus } from '@features/payments';
 
type FilterStatus = 'all' | PaymentStatus;
 
const STATUS_FILTERS: { key: FilterStatus; label: string }[] = [
  { key: 'all', label: 'All' },
  { key: 'Pending', label: 'Pending' },
  { key: 'Authorized', label: 'Authorized' },
  { key: 'Captured', label: 'Captured' },
  { key: 'Failed', label: 'Failed' },
  { key: 'Refunded', label: 'Refunded' },
  { key: 'Cancelled', label: 'Cancelled' },
];
 
export function PaymentsListPage() {
  const { t } = useTranslation();
  const [statusFilter, setStatusFilter] = useState<FilterStatus>('all');
  const [selectedId, setSelectedId] = useState<string | null>(null);
 
  const { data: payments, isLoading } = usePayments({
    status: statusFilter === 'all' ? undefined : statusFilter,
  });
 
  // Summary stats
  const total = payments?.length ?? 0;
  const capturedCount = payments?.filter((p) => p.status === 'Captured').length ?? 0;
  const pendingCount = payments?.filter((p) => p.status === 'Pending' || p.status === 'Authorized').length ?? 0;
  const failedCount = payments?.filter((p) => p.status === 'Failed' || p.status === 'Cancelled').length ?? 0;
  const totalRevenue = payments?.filter((p) => p.status === 'Captured').reduce((s, p) => s + p.netAmount, 0) ?? 0;
 
  const summaryCards = [
    { label: 'Total Payments', value: total.toString(), icon: CreditCard, color: 'text-primary-600 dark:text-primary-400', bg: 'bg-primary-100 dark:bg-primary-500/15' },
    { label: 'Captured', value: capturedCount.toString(), icon: CheckCircle, color: 'text-emerald-600 dark:text-emerald-400', bg: 'bg-emerald-100 dark:bg-emerald-500/15' },
    { label: 'Pending', value: pendingCount.toString(), icon: Clock, color: 'text-amber-600 dark:text-amber-400', bg: 'bg-amber-100 dark:bg-amber-500/15' },
    { label: 'Revenue', value: `$${totalRevenue.toLocaleString('en-US', { minimumFractionDigits: 2 })}`, icon: CheckCircle, color: 'text-emerald-600 dark:text-emerald-400', bg: 'bg-emerald-100 dark:bg-emerald-500/15' },
  ];
 
  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-[var(--text-primary)]">{t('nav.payments')}</h1>
        <p className="mt-1 text-sm text-[var(--text-secondary)]">Track payment lifecycle, refunds, and gateway transactions</p>
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
 
      {/* Status filters */}
      <Card>
        <div className="flex flex-wrap gap-2">
          {STATUS_FILTERS.map(({ key, label }) => (
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
        ) : !payments?.length ? (
          <EmptyState
            icon={<CreditCard className="h-12 w-12" />}
            title="No payments found"
            description="Payments will appear here when orders are processed."
          />
        ) : (
          <div className="overflow-x-auto -mx-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Payment</th>
                  <th className="hidden md:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Method</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">Amount</th>
                  <th className="hidden lg:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">Net</th>
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('common.status')}</th>
                  <th className="hidden sm:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Gateway</th>
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