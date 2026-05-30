import { useTranslation } from 'react-i18next';
import { Eye } from 'lucide-react';
import { Badge, ActionMenu } from '@shared/components/ui';
import { formatDate } from '@shared/lib/utils';
import { cn } from '@shared/lib/utils';
import { paymentStatusVariant, type Payment } from '@features/payments';

interface PaymentRowProps {
  payment: Payment;
  onViewDetail: () => void;
}

export function PaymentRow({ payment, onViewDetail }: PaymentRowProps) {
  const { t } = useTranslation();

  return (
    <tr className="group hover:bg-[var(--bg-tertiary)]/50 transition-colors">
      {/* Payment info */}
      <td className="px-6 py-3.5">
        <div className="min-w-0">
          <p className="font-medium text-[var(--text-primary)] font-mono text-xs">
            {payment.id.substring(0, 8)}...
          </p>
          <p className="text-xs text-[var(--text-muted)]">
            Order {payment.orderId.substring(0, 8)} · {formatDate(payment.createdAt)}
          </p>
        </div>
      </td>
      {/* Method */}
      <td className="hidden md:table-cell px-6 py-3.5">
        <p className="text-sm text-[var(--text-primary)]">{payment.methodLabel}</p>
      </td>
      {/* Amount */}
      <td className="px-6 py-3.5 text-right font-semibold text-[var(--text-primary)] tabular-nums">
        {payment.formattedAmount}
      </td>
      {/* Net (after refunds) */}
      <td className="hidden lg:table-cell px-6 py-3.5 text-right tabular-nums">
        <span className={cn(
          'font-semibold',
          payment.hasRefunds ? 'text-amber-600 dark:text-amber-400' : 'text-[var(--text-primary)]'
        )}>
          {payment.formattedNet}
        </span>
        {payment.hasRefunds && (
          <p className="text-xs text-[var(--text-muted)]">-{payment.totalRefunded.toFixed(2)} refunded</p>
        )}
      </td>
      {/* Status */}
      <td className="px-6 py-3.5">
        <Badge variant={paymentStatusVariant(payment.status)}>{payment.status}</Badge>
      </td>
      {/* Gateway */}
      <td className="hidden sm:table-cell px-6 py-3.5 text-[var(--text-secondary)]">
        {payment.gatewayName ?? '—'}
      </td>
      {/* Actions */}
      <td className="px-6 py-3.5 text-right">
        <ActionMenu
          items={[
            { label: t('common.viewDetails'), icon: Eye, onClick: onViewDetail },
          ]}
        />
      </td>
    </tr>
  );
}
