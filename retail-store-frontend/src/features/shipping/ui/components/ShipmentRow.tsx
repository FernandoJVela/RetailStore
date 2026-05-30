import { MoreHorizontal, Eye } from 'lucide-react';
import { Badge } from '@shared/components/ui';
import { formatDate } from '@shared/lib/utils';
import { shipmentStatusVariant } from '@features/shipping';
import type { Shipment } from '@features/shipping';
import { useState, useRef, useEffect } from 'react';
 
interface ShipmentRowProps {
  shipment: Shipment;
  onViewDetail: () => void;
}
 
export function ShipmentRow({ shipment, onViewDetail }: ShipmentRowProps) {
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
 
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) setMenuOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);
 
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
        <div className="relative" ref={menuRef}>
          <button onClick={() => setMenuOpen(!menuOpen)} className="rounded-lg p-1.5 text-[var(--text-muted)] hover:bg-[var(--bg-tertiary)] transition-colors">
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