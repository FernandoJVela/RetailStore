import { useTranslation } from 'react-i18next';
import { Eye, UserX, UserCheck } from 'lucide-react';
import { Badge, ActionMenu, Avatar } from '@shared/components/ui';
import {
  useDeactivateProvider,
  useReactivateProvider,
  type Provider,
} from '@features/providers';

interface ProviderRowProps {
  provider: Provider;
  onViewDetail: () => void;
}

export function ProviderRow({ provider, onViewDetail }: ProviderRowProps) {
  const { t } = useTranslation();
  const deactivate = useDeactivateProvider();
  const reactivate = useReactivateProvider();

  const handleToggle = async () => {
    if (provider.isActive) await deactivate.mutateAsync(provider.id);
    else await reactivate.mutateAsync(provider.id);
  };

  const initials = provider.companyName.substring(0, 2).toUpperCase();

  return (
    <tr className="group hover:bg-[var(--bg-tertiary)]/50 transition-colors">
      {/* Company */}
      <td className="px-6 py-3.5">
        <div className="flex items-center gap-3">
          <Avatar initials={initials} variant="amber" shape="square" />
          <div className="min-w-0">
            <p className="font-medium text-[var(--text-primary)] truncate">{provider.companyName}</p>
            <p className="text-xs text-[var(--text-muted)] md:hidden">{provider.contactName}</p>
          </div>
        </div>
      </td>
      {/* Contact */}
      <td className="hidden md:table-cell px-6 py-3.5 text-[var(--text-secondary)]">{provider.contactName}</td>
      {/* Email */}
      <td className="hidden lg:table-cell px-6 py-3.5 text-[var(--text-secondary)]">{provider.email}</td>
      {/* Product count */}
      <td className="px-6 py-3.5 text-center">
        <Badge variant={provider.productCount > 0 ? 'info' : 'default'}>
          {provider.productCount} {provider.productCount === 1 ? 'product' : 'products'}
        </Badge>
      </td>
      {/* Status */}
      <td className="px-6 py-3.5">
        <Badge variant={provider.isActive ? 'success' : 'danger'}>{provider.statusLabel}</Badge>
      </td>
      {/* Actions */}
      <td className="px-6 py-3.5 text-right">
        <ActionMenu
          items={[
            {
              label: t('common.viewDetails'),
              icon: Eye,
              onClick: onViewDetail,
            },
            {
              label: provider.isActive ? t('users.deactivate') : t('users.reactivate'),
              icon: provider.isActive ? UserX : UserCheck,
              onClick: handleToggle,
              variant: provider.isActive ? 'danger' : 'success',
            },
          ]}
        />
      </td>
    </tr>
  );
}
