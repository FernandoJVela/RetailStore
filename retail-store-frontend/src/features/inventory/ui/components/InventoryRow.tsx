import { useTranslation } from 'react-i18next';
import { Plus, Minus, ArrowLeftRight, Settings2 } from 'lucide-react';
import { Badge, ActionMenu } from '@shared/components/ui';
import { cn } from '@shared/lib/utils';
import { stockStatusVariant } from '@features/inventory';
import type { InventoryItem } from '@features/inventory';

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
  const { t } = useTranslation();

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
        <ActionMenu
          items={[
            {
              label: t('inventory.addStock'),
              icon: Plus,
              iconColor: 'text-emerald-600',
              onClick: () => onAction('add'),
            },
            {
              label: t('inventory.removeStock'),
              icon: Minus,
              iconColor: 'text-red-600',
              onClick: () => onAction('remove'),
            },
            {
              label: t('inventory.adjustQuantity'),
              icon: ArrowLeftRight,
              iconColor: 'text-primary-600',
              onClick: () => onAction('adjust'),
            },
            {
              label: t('inventory.updateThreshold'),
              icon: Settings2,
              iconColor: 'text-[var(--text-muted)]',
              onClick: () => onAction('threshold'),
              separator: true,
            },
          ]}
        />
      </td>
    </tr>
  );
}
