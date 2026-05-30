import { useTranslation } from 'react-i18next';
import { Eye } from 'lucide-react';
import { Badge, ActionMenu } from '@shared/components/ui';
import { formatDate } from '@shared/lib/utils';
import { orderStatusVariant } from '@features/orders';
import type { Order } from '@features/orders';

interface OrderRowProps {
  order: Order;
  onViewDetail: () => void;
}

export function OrderRow({ order, onViewDetail }: OrderRowProps) {
  const { t } = useTranslation();

  return (
    <tr className="group hover:bg-[var(--bg-tertiary)]/50 transition-colors">
      <td className="px-6 py-3.5">
        <div className="min-w-0">
          <p className="font-medium text-[var(--text-primary)] font-mono text-xs">{order.id.substring(0, 8)}...</p>
          <p className="text-xs text-[var(--text-muted)] sm:hidden">{formatDate(order.orderDate)}</p>
        </div>
      </td>
      <td className="hidden sm:table-cell px-6 py-3.5 text-[var(--text-secondary)]">{formatDate(order.orderDate)}</td>
      <td className="px-6 py-3.5 text-center tabular-nums text-[var(--text-primary)]">{order.itemCount}</td>
      <td className="px-6 py-3.5 text-right font-semibold text-[var(--text-primary)] tabular-nums">{order.formattedTotal}</td>
      <td className="px-6 py-3.5">
        <Badge variant={orderStatusVariant(order.status)}>{order.status}</Badge>
      </td>
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
