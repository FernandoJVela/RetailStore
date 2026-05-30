import { useTranslation } from 'react-i18next';
import { Eye, PackageX, PackageCheck } from 'lucide-react';
import { Badge, ActionMenu } from '@shared/components/ui';
import { useDeactivateProduct, useReactivateProduct } from '@features/products/application/hooks/useProductsQueries';
import type { Product } from '@features/products';

interface ProductRowProps {
  product: Product;
  onViewDetail: () => void;
}

export function ProductRow({ product, onViewDetail }: ProductRowProps) {
  const { t } = useTranslation();
  const deactivate = useDeactivateProduct();
  const reactivate = useReactivateProduct();

  const handleToggle = async () => {
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
        <ActionMenu
          items={[
            {
              label: t('common.viewDetails'),
              icon: Eye,
              onClick: onViewDetail,
            },
            {
              label: product.isActive ? t('users.deactivate') : t('users.reactivate'),
              icon: product.isActive ? PackageX : PackageCheck,
              onClick: handleToggle,
              variant: product.isActive ? 'danger' : 'success',
            },
          ]}
        />
      </td>
    </tr>
  );
}
