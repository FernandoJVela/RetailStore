import { useTranslation } from 'react-i18next';
import { Eye, UserX, UserCheck } from 'lucide-react';
import { Badge, ActionMenu, Avatar } from '@shared/components/ui';
import { formatDate } from '@shared/lib/utils';
import { useDeactivateCustomer, useReactivateCustomer } from '@features/customers/application/hooks/useCustomersQueries';
import type { Customer } from '@features/customers';

interface CustomerRowProps {
  customer: Customer;
  onViewDetail: () => void;
}

export function CustomerRow({ customer, onViewDetail }: CustomerRowProps) {
  const { t } = useTranslation();
  const deactivate = useDeactivateCustomer();
  const reactivate = useReactivateCustomer();

  const handleToggle = async () => {
    if (customer.isActive) await deactivate.mutateAsync(customer.id);
    else await reactivate.mutateAsync(customer.id);
  };

  const initials = `${customer.firstName.charAt(0)}${customer.lastName.charAt(0)}`.toUpperCase();

  return (
    <tr className="group hover:bg-[var(--bg-tertiary)]/50 transition-colors">
      {/* Name + avatar (always visible) */}
      <td className="px-6 py-3.5">
        <div className="flex items-center gap-3">
          <Avatar initials={initials} />
          <div className="min-w-0">
            <p className="font-medium text-[var(--text-primary)] truncate">{customer.fullName}</p>
            {/* Show email on mobile under the name */}
            <p className="text-xs text-[var(--text-muted)] md:hidden truncate">{customer.email}</p>
          </div>
        </div>
      </td>
      {/* Email (hidden on mobile) */}
      <td className="hidden md:table-cell px-6 py-3.5 text-[var(--text-secondary)]">{customer.email}</td>
      {/* Phone (hidden on tablet) */}
      <td className="hidden lg:table-cell px-6 py-3.5 text-[var(--text-secondary)]">{customer.phone || '—'}</td>
      {/* Status */}
      <td className="px-6 py-3.5">
        <Badge variant={customer.isActive ? 'success' : 'danger'}>
          {customer.isActive ? t('common.active') : t('common.inactive')}
        </Badge>
      </td>
      {/* Date (hidden on small mobile) */}
      <td className="hidden sm:table-cell px-6 py-3.5 text-[var(--text-secondary)]">
        {formatDate(customer.createdAt)}
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
              label: customer.isActive ? t('users.deactivate') : t('users.reactivate'),
              icon: customer.isActive ? UserX : UserCheck,
              onClick: handleToggle,
              variant: customer.isActive ? 'danger' : 'success',
            },
          ]}
        />
      </td>
    </tr>
  );
}
