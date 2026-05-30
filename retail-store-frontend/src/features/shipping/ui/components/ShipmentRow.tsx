import { useTranslation } from 'react-i18next';
import { Eye } from 'lucide-react';
import { Badge, ActionMenu } from '@shared/components/ui';
import { formatDate } from '@shared/lib/utils';
import { shipmentStatusVariant } from '@features/shipping';
import type { Shipment } from '@features/shipping';

interface ShipmentRowProps {
  shipment: Shipment;
  onViewDetail: () => void;
}

export function ShipmentRow({ shipment, onViewDetail }: ShipmentRowProps) {
  const { t } = useTranslation();

  const statusLabel = shipment.status === 'InTransit' ? 'In Transit' : shipment.status;

  return (
    <tr className="group hover:bg-[var(--bg-tertiary)]/50 transition-colors">
      {/* Shipment info */}
      <td className="px-6 py-3.5">
        <div className="min-w-0">
          <p className="font-medium text-[var(--text-primary)] font-mono text-xs">{shipment.id.substring(0, 8)}...</p>
          <p className="text-xs text-[var(--text-muted)]">Order {shipment.orderId.substring(0, 8)} · {formatDate(shipment.createdAt)}</p>
        </div>
      </td>
      {/* Carrier */}
      <td className="hidden md:table-cell px-6 py-3.5 text-[var(--text-secondary)]">
        {shipment.carrier ?? <span className="italic text-[var(--text-muted)]">Not assigned</span>}
      </td>
      {/* Tracking */}
      <td className="hidden lg:table-cell px-6 py-3.5">
        {shipment.trackingNumber ? (
          <span className="font-mono text-xs text-[var(--text-primary)]">{shipment.trackingNumber}</span>
        ) : (
          <span className="text-[var(--text-muted)] italic">—</span>
        )}
      </td>
      {/* Items */}
      <td className="px-6 py-3.5 text-center tabular-nums text-[var(--text-primary)]">{shipment.itemCount}</td>
      {/* Cost */}
      <td className="hidden sm:table-cell px-6 py-3.5 text-right tabular-nums text-[var(--text-primary)]">{shipment.formattedCost}</td>
      {/* Status */}
      <td className="px-6 py-3.5">
        <Badge variant={shipmentStatusVariant(shipment.status)}>{statusLabel}</Badge>
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
