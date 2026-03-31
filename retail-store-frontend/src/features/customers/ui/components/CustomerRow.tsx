import { useTranslation } from 'react-i18next';
import { MoreHorizontal, Eye, UserX, UserCheck } from 'lucide-react';
import { Badge } from '@shared/components/ui';
import { formatDate } from '@shared/lib/utils';
import { useDeactivateCustomer, useReactivateCustomer } from '@features/customers/application/hooks/useCustomersQueries';
import type { Customer } from '@features/customers';
import { useState, useRef, useEffect } from 'react';
 
interface CustomerRowProps {
  customer: Customer;
  onViewDetail: () => void;
}
 
export function CustomerRow({ customer, onViewDetail }: CustomerRowProps) {
  const { t } = useTranslation();
  const deactivate = useDeactivateCustomer();
  const reactivate = useReactivateCustomer();
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
    if (customer.isActive) await deactivate.mutateAsync(customer.id);
    else await reactivate.mutateAsync(customer.id);
  };
 
  const initials = `${customer.firstName.charAt(0)}${customer.lastName.charAt(0)}`.toUpperCase();
 
  return (
    <tr className="group hover:bg-[var(--bg-tertiary)]/50 transition-colors">
      {/* Name + avatar (always visible) */}
      <td className="px-6 py-3.5">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary-100 dark:bg-primary-500/15 text-sm font-bold text-primary-700 dark:text-primary-400">
            {initials}
          </div>
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
                  customer.isActive
                    ? 'text-red-600 hover:bg-red-50 dark:hover:bg-red-500/10'
                    : 'text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-500/10'
                }`}
              >
                {customer.isActive ? (
                  <><UserX className="h-4 w-4" /> {t('users.deactivate')}</>
                ) : (
                  <><UserCheck className="h-4 w-4" /> {t('users.reactivate')}</>
                )}
              </button>
            </div>
          )}
        </div>
      </td>
    </tr>
  );
}