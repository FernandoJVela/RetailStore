import { useTranslation } from 'react-i18next';
import { MoreHorizontal, Eye, UserX, UserCheck } from 'lucide-react';
import { Badge } from '@shared/components/ui';
import { 
  useDeactivateProvider, 
  useReactivateProvider, 
  type Provider } from '@features/providers';
import { useState, useRef, useEffect } from 'react';
 
interface ProviderRowProps {
  provider: Provider;
  onViewDetail: () => void;
}
 
export function ProviderRow({ provider, onViewDetail }: ProviderRowProps) {
  const { t } = useTranslation();
  const deactivate = useDeactivateProvider();
  const reactivate = useReactivateProvider();
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
    if (provider.isActive) await deactivate.mutateAsync(provider.id);
    else await reactivate.mutateAsync(provider.id);
  };
 
  const initials = provider.companyName.substring(0, 2).toUpperCase();
 
  return (
    <tr className="group hover:bg-[var(--bg-tertiary)]/50 transition-colors">
      {/* Company */}
      <td className="px-6 py-3.5">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-amber-100 dark:bg-amber-500/15 text-sm font-bold text-amber-700 dark:text-amber-400">
            {initials}
          </div>
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
                  provider.isActive
                    ? 'text-red-600 hover:bg-red-50 dark:hover:bg-red-500/10'
                    : 'text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-500/10'
                }`}
              >
                {provider.isActive ? (
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