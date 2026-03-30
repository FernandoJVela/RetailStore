import { useTranslation } from 'react-i18next';
import { MoreHorizontal, Eye, PackageX, PackageCheck } from 'lucide-react';
import { Badge } from '@shared/components/ui';
import { useDeactivateProduct, useReactivateProduct } from '@features/products/application/hooks/useProductsQueries';
import type { Product } from '@features/products';
import { useState, useRef, useEffect } from 'react';
 
interface ProductRowProps {
  product: Product;
  onViewDetail: () => void;
}
 
export function ProductRow({ product, onViewDetail }: ProductRowProps) {
  const { t } = useTranslation();
  const deactivate = useDeactivateProduct();
  const reactivate = useReactivateProduct();
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
 
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) setMenuOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);
 
  const handleToggle = async () => {
    setMenuOpen(false);
    if (product.isActive) await deactivate.mutateAsync(product.id);
    else await reactivate.mutateAsync(product.id);
  };
 
  return (
    <tr className="group hover:bg-[var(--bg-tertiary)]/50 transition-colors">
      <td className="px-6 py-3.5">
        <div className="min-w-0">
          <p className="font-medium text-[var(--text-primary)] truncate">{product.name}</p>
          <p className="text-xs text-[var(--text-muted)] font-mono md:hidden">{product.sku}</p>
        </div>
      </td>
      <td className="hidden md:table-cell px-6 py-3.5 text-[var(--text-secondary)] font-mono text-xs">{product.sku}</td>
      <td className="px-6 py-3.5 font-semibold text-[var(--text-primary)]">{product.formattedPrice}</td>
      <td className="hidden lg:table-cell px-6 py-3.5">
        <Badge variant="default">{product.category}</Badge>
      </td>
      <td className="px-6 py-3.5">
        <Badge variant={product.isActive ? 'success' : 'danger'}>{product.statusLabel}</Badge>
      </td>
      <td className="px-6 py-3.5 text-right">
        <div className="relative" ref={menuRef}>
          <button
            onClick={() => setMenuOpen(!menuOpen)}
            className="rounded-lg p-1.5 text-[var(--text-muted)] hover:bg-[var(--bg-tertiary)] transition-colors"
          >
            <MoreHorizontal className="h-5 w-5" />
          </button>
          {menuOpen && (
            <div className="absolute right-0 top-full z-10 mt-1 w-48 rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] py-1 shadow-lg">
              <button
                onClick={() => { onViewDetail(); setMenuOpen(false); }}
                className="flex w-full items-center gap-2 px-4 py-2.5 text-sm text-[var(--text-primary)] hover:bg-[var(--bg-tertiary)]"
              >
                <Eye className="h-4 w-4" /> View Details
              </button>
              <button
                onClick={handleToggle}
                className={`flex w-full items-center gap-2 px-4 py-2.5 text-sm ${
                  product.isActive
                    ? 'text-red-600 hover:bg-red-50 dark:hover:bg-red-500/10'
                    : 'text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-500/10'
                }`}
              >
                {product.isActive ? (
                  <><PackageX className="h-4 w-4" /> {t('users.deactivate')}</>
                ) : (
                  <><PackageCheck className="h-4 w-4" /> {t('users.reactivate')}</>
                )}
              </button>
            </div>
          )}
        </div>
      </td>
    </tr>
  );
}