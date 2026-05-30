import { MoreHorizontal, Eye } from 'lucide-react';
import { Badge } from '@shared/components/ui';
import { formatDate } from '@shared/lib/utils';
import { orderStatusVariant } from '@features/orders';
import type { Order } from '@features/orders';
import { useState, useRef, useEffect } from 'react';
 
interface OrderRowProps {
  order: Order;
  onViewDetail: () => void;
}
 
export function OrderRow({ order, onViewDetail }: OrderRowProps) {
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