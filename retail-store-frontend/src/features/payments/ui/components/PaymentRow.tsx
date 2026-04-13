import { MoreHorizontal, Eye } from 'lucide-react';
import { Badge } from '@shared/components/ui';
import { formatDate } from '@shared/lib/utils';
import { cn } from '@shared/lib/utils';
import { paymentStatusVariant, type Payment } from '@features/payments';
import { useState, useRef, useEffect } from 'react';
 
interface PaymentRowProps {
  payment: Payment;
  onViewDetail: () => void;
}
 
export function PaymentRow({ payment, onViewDetail }: PaymentRowProps) {
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
 
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) setMenuOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);
 
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
        <div className="relative" ref={menuRef}>
          <button
            onClick={() => setMenuOpen(!menuOpen)}
            className="rounded-lg p-1.5 text-[var(--text-muted)] hover:bg-[var(--bg-tertiary)] transition-colors"
          >
            <MoreHorizontal className="h-5 w-5" />
          </button>
          {menuOpen && (
            <div className="absolute right-0 top-full z-10 mt-1 w-44 rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] py-1 shadow-lg">
              <button
                onClick={() => { onViewDetail(); setMenuOpen(false); }}
                className="flex w-full items-center gap-2 px-4 py-2.5 text-sm text-[var(--text-primary)] hover:bg-[var(--bg-tertiary)]"
              >
                <Eye className="h-4 w-4" /> View Details
              </button>
            </div>
          )}
        </div>
      </td>
    </tr>
  );
}