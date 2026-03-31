import { MoreHorizontal, Plus, Minus, ArrowLeftRight, Settings2 } from 'lucide-react';
import { Badge } from '@shared/components/ui';
import { cn } from '@shared/lib/utils';
import { stockStatusVariant } from '@features/inventory';
import type { InventoryItem } from '@features/inventory';
import { useState, useRef, useEffect } from 'react';
 
interface InventoryRowProps {
  item: InventoryItem;
  onAction: (action: 'add' | 'remove' | 'adjust' | 'threshold') => void;
}
 
function healthBarColor(percent: number): string {
  if (percent >= 60) return 'bg-emerald-500';
  if (percent >= 30) return 'bg-amber-500';
  return 'bg-red-500';
}
 
export function InventoryRow({ item, onAction }: InventoryRowProps) {
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
 
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) setMenuOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);
 
  const handleAction = (action: 'add' | 'remove' | 'adjust' | 'threshold') => {
    setMenuOpen(false);
    onAction(action);
  };
 
  return (
    <tr className={cn('group transition-colors', item.isOutOfStock ? 'bg-red-50/50 dark:bg-red-500/5' : 'hover:bg-[var(--bg-tertiary)]/50')}>
      {/* Product */}
      <td className="px-6 py-3.5">
        <div className="min-w-0">
          <p className="font-medium text-[var(--text-primary)] truncate">{item.productName}</p>
          <p className="text-xs text-[var(--text-muted)] font-mono">{item.sku}</p>
        </div>
      </td>
      {/* On Hand */}
      <td className="px-6 py-3.5 text-right font-semibold text-[var(--text-primary)] tabular-nums">
        {item.quantityOnHand.toLocaleString()}
      </td>
      {/* Reserved */}
      <td className="hidden md:table-cell px-6 py-3.5 text-right text-[var(--text-secondary)] tabular-nums">
        {item.reservedQuantity.toLocaleString()}
      </td>
      {/* Available */}
      <td className="px-6 py-3.5 text-right tabular-nums">
        <span className={cn('font-semibold', item.isOutOfStock ? 'text-red-600 dark:text-red-400' : item.isLowStock ? 'text-amber-600 dark:text-amber-400' : 'text-[var(--text-primary)]')}>
          {item.availableQuantity.toLocaleString()}
        </span>
      </td>
      {/* Threshold */}
      <td className="hidden lg:table-cell px-6 py-3.5 text-right text-[var(--text-secondary)] tabular-nums">
        {item.reorderThreshold}
      </td>
      {/* Status */}
      <td className="px-6 py-3.5">
        <Badge variant={stockStatusVariant(item.stockStatus)}>
          {item.stockStatus === 'InStock' ? 'In Stock' : item.stockStatus === 'LowStock' ? 'Low Stock' : 'Out of Stock'}
        </Badge>
      </td>
      {/* Health bar */}
      <td className="hidden sm:table-cell px-6 py-3.5">
        <div className="flex items-center gap-2">
          <div className="h-2 w-20 rounded-full bg-[var(--bg-tertiary)] overflow-hidden">
            <div
              className={cn('h-full rounded-full transition-all', healthBarColor(item.stockHealthPercent))}
              style={{ width: `${item.stockHealthPercent}%` }}
            />
          </div>
          <span className="text-xs text-[var(--text-muted)] tabular-nums w-8">{item.stockHealthPercent}%</span>
        </div>
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
            <div className="absolute right-0 top-full z-10 mt-1 w-52 rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] py-1 shadow-lg">
              <button onClick={() => handleAction('add')} className="flex w-full items-center gap-2 px-4 py-2.5 text-sm text-[var(--text-primary)] hover:bg-[var(--bg-tertiary)]">
                <Plus className="h-4 w-4 text-emerald-600" /> Add Stock
              </button>
              <button onClick={() => handleAction('remove')} className="flex w-full items-center gap-2 px-4 py-2.5 text-sm text-[var(--text-primary)] hover:bg-[var(--bg-tertiary)]">
                <Minus className="h-4 w-4 text-red-600" /> Remove Stock
              </button>
              <button onClick={() => handleAction('adjust')} className="flex w-full items-center gap-2 px-4 py-2.5 text-sm text-[var(--text-primary)] hover:bg-[var(--bg-tertiary)]">
                <ArrowLeftRight className="h-4 w-4 text-primary-600" /> Adjust Quantity
              </button>
              <div className="my-1 border-t border-[var(--border-color)]" />
              <button onClick={() => handleAction('threshold')} className="flex w-full items-center gap-2 px-4 py-2.5 text-sm text-[var(--text-primary)] hover:bg-[var(--bg-tertiary)]">
                <Settings2 className="h-4 w-4 text-[var(--text-muted)]" /> Update Threshold
              </button>
            </div>
          )}
        </div>
      </td>
    </tr>
  );
}