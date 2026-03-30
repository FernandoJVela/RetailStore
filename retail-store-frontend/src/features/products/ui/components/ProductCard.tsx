import { Package } from 'lucide-react';
import { Badge } from '@shared/components/ui';
import { cn } from '@shared/lib/utils';
import type { Product } from '@features/products';
 
interface ProductCardProps {
  product: Product;
  onClick: () => void;
}
 
const categoryColors: Record<string, string> = {
  Electronics: 'bg-blue-100 text-blue-800 dark:bg-blue-500/15 dark:text-blue-400',
  Furniture: 'bg-amber-100 text-amber-800 dark:bg-amber-500/15 dark:text-amber-400',
  Stationery: 'bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-400',
};
 
export function ProductCard({ product, onClick }: ProductCardProps) {
  return (
    <button
      onClick={onClick}
      className={cn(
        'flex flex-col rounded-xl border bg-[var(--bg-secondary)] p-5 text-left transition-all hover:shadow-md hover:border-primary-300 dark:hover:border-primary-700',
        product.isActive ? 'border-[var(--border-color)]' : 'border-[var(--border-color)] opacity-60'
      )}
    >
      {/* Icon + category */}
      <div className="flex items-start justify-between">
        <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary-100 dark:bg-primary-500/15">
          <Package className="h-5 w-5 text-primary-600 dark:text-primary-400" />
        </div>
        <span className={cn('rounded-full px-2 py-0.5 text-xs font-medium', categoryColors[product.category] || categoryColors['Electronics'])}>
          {product.category}
        </span>
      </div>
 
      {/* Name + SKU */}
      <div className="mt-4 min-w-0">
        <h3 className="font-semibold text-[var(--text-primary)] truncate">{product.name}</h3>
        <p className="mt-0.5 text-xs text-[var(--text-muted)] font-mono">{product.sku}</p>
      </div>
 
      {/* Price + status */}
      <div className="mt-4 flex items-end justify-between">
        <span className="text-xl font-bold text-[var(--text-primary)]">{product.formattedPrice}</span>
        <Badge variant={product.isActive ? 'success' : 'danger'}>
          {product.statusLabel}
        </Badge>
      </div>
    </button>
  );
}