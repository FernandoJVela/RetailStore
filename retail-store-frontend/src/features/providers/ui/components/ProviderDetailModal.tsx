import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslation } from 'react-i18next';
import { X, Pencil, Mail, Phone, Package, Link2Off } from 'lucide-react';
import { Button, Input, Badge, Spinner } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import { formatDate } from '@shared/lib/utils';
import {
  useProvider, useUpdateProvider, useChangeProviderEmail,
  useAssociateProduct, useDissociateProduct,
  updateProviderSchema, changeEmailSchema,
  type UpdateProviderFormData, type ChangeEmailFormData,
} from '@features/providers';
import { useProducts } from '@features/products';
 
interface ProviderDetailPanelProps {
  providerId: string;
  isOpen: boolean;
  onClose: () => void;
}
 
type EditMode = 'none' | 'profile' | 'email';
 
export function ProviderDetailPanel({ providerId, isOpen, onClose }: ProviderDetailPanelProps) {
  const { t } = useTranslation();
  const { data: provider, isLoading } = useProvider(providerId);
  const { data: allProducts } = useProducts({ isActive: true });
  const [editMode, setEditMode] = useState<EditMode>('none');
  const [apiError, setApiError] = useState('');
 
  const updateMutation = useUpdateProvider();
  const emailMutation = useChangeProviderEmail();
  const associateMutation = useAssociateProduct();
  const dissociateMutation = useDissociateProduct();
 
  // Profile form
  const profileForm = useForm<UpdateProviderFormData>({
    resolver: yupResolver(updateProviderSchema),
    values: provider ? { companyName: provider.companyName, contactName: provider.contactName, phone: provider.phone || '' } : undefined,
  });
 
  const handleProfileSave = async (data: UpdateProviderFormData) => {
    setApiError('');
    try {
      await updateMutation.mutateAsync({ id: providerId, data });
      setEditMode('none');
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  // Email form
  const emailForm = useForm<ChangeEmailFormData>({
    resolver: yupResolver(changeEmailSchema),
    values: provider ? { newEmail: provider.email } : undefined,
  });
 
  const handleEmailSave = async (data: ChangeEmailFormData) => {
    setApiError('');
    try {
      await emailMutation.mutateAsync({ id: providerId, newEmail: data.newEmail });
      setEditMode('none');
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  // Product association
  const associatedIds = new Set(provider?.productIds ?? []);
  const unassociatedProducts = allProducts?.filter((p) => !associatedIds.has(p.id)) ?? [];
  const associatedProducts = allProducts?.filter((p) => associatedIds.has(p.id)) ?? [];
 
  const handleAssociate = async (productId: string) => {
    try {
      await associateMutation.mutateAsync({ providerId, productId });
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  const handleDissociate = async (productId: string) => {
    try {
      await dissociateMutation.mutateAsync({ providerId, productId });
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  if (!isOpen) return null;
 
  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm" onClick={onClose} />
      <div className="fixed inset-y-0 right-0 z-50 w-full max-w-lg overflow-y-auto bg-[var(--bg-secondary)] shadow-2xl">
        {/* Header */}
        <div className="sticky top-0 z-10 flex items-center justify-between border-b border-[var(--border-color)] bg-[var(--bg-secondary)] px-6 py-4">
          <h2 className="text-lg font-semibold text-[var(--text-primary)]">Provider Details</h2>
          <button onClick={onClose} className="rounded-lg p-1.5 text-[var(--text-muted)] hover:bg-[var(--bg-tertiary)]">
            <X className="h-5 w-5" />
          </button>
        </div>
 
        {isLoading ? (
          <Spinner />
        ) : provider ? (
          <div className="p-6 space-y-6">
            {apiError && (
              <div className="rounded-lg bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-800 p-3">
                <p className="text-sm text-red-700 dark:text-red-400">{apiError}</p>
              </div>
            )}
 
            {/* ─── Profile Section ────────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider">Company Info</h3>
                <button onClick={() => setEditMode(editMode === 'profile' ? 'none' : 'profile')} className="text-primary-600 hover:text-primary-500">
                  <Pencil className="h-4 w-4" />
                </button>
              </div>
 
              {editMode === 'profile' ? (
                <form onSubmit={profileForm.handleSubmit(handleProfileSave)} className="space-y-3">
                  <Input label="Company Name" error={profileForm.formState.errors.companyName?.message} {...profileForm.register('companyName')} />
                  <Input label="Contact Name" error={profileForm.formState.errors.contactName?.message} {...profileForm.register('contactName')} />
                  <Input label="Phone" error={profileForm.formState.errors.phone?.message} {...profileForm.register('phone')} />
                  <div className="flex justify-end gap-2 pt-1">
                    <Button variant="secondary" size="sm" type="button" onClick={() => setEditMode('none')}>{t('common.cancel')}</Button>
                    <Button size="sm" type="submit" loading={updateMutation.isPending}>{t('common.save')}</Button>
                  </div>
                </form>
              ) : (
                <div className="space-y-3">
                  <div className="flex items-center gap-3">
                    <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-amber-100 dark:bg-amber-500/15 text-lg font-bold text-amber-700 dark:text-amber-400">
                      {provider.companyName.substring(0, 2).toUpperCase()}
                    </div>
                    <div>
                      <p className="font-semibold text-[var(--text-primary)]">{provider.companyName}</p>
                      <p className="text-sm text-[var(--text-secondary)]">{provider.contactName}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <Badge variant={provider.isActive ? 'success' : 'danger'}>{provider.statusLabel}</Badge>
                    {provider.phone && (
                      <span className="flex items-center gap-1 text-sm text-[var(--text-secondary)]">
                        <Phone className="h-3.5 w-3.5" /> {provider.phone}
                      </span>
                    )}
                  </div>
                  <p className="text-xs text-[var(--text-muted)]">Registered {formatDate(provider.createdAt)}</p>
                </div>
              )}
            </section>
 
            {/* ─── Email Section ──────────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider">Email</h3>
                <button onClick={() => setEditMode(editMode === 'email' ? 'none' : 'email')} className="text-primary-600 hover:text-primary-500">
                  <Pencil className="h-4 w-4" />
                </button>
              </div>
 
              {editMode === 'email' ? (
                <form onSubmit={emailForm.handleSubmit(handleEmailSave)} className="space-y-3">
                  <Input label="New Email" type="email" error={emailForm.formState.errors.newEmail?.message} {...emailForm.register('newEmail')} />
                  <div className="flex justify-end gap-2 pt-1">
                    <Button variant="secondary" size="sm" type="button" onClick={() => setEditMode('none')}>{t('common.cancel')}</Button>
                    <Button size="sm" type="submit" loading={emailMutation.isPending}>{t('common.save')}</Button>
                  </div>
                </form>
              ) : (
                <div className="flex items-center gap-2 text-sm text-[var(--text-secondary)]">
                  <Mail className="h-4 w-4" /> {provider.email}
                </div>
              )}
            </section>
 
            {/* ─── Associated Products Section ────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <div className="mb-4">
                <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider flex items-center gap-2">
                  <Package className="h-4 w-4" /> Associated Products ({associatedProducts.length})
                </h3>
              </div>
 
              {/* Currently associated */}
              {associatedProducts.length > 0 ? (
                <div className="space-y-2 mb-4">
                  {associatedProducts.map((product) => (
                    <div key={product.id} className="flex items-center justify-between rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] px-3 py-2">
                      <div className="min-w-0">
                        <p className="text-sm font-medium text-[var(--text-primary)] truncate">{product.name}</p>
                        <p className="text-xs text-[var(--text-muted)] font-mono">{product.sku}</p>
                      </div>
                      <button
                        onClick={() => handleDissociate(product.id)}
                        className="shrink-0 rounded-lg p-1.5 text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10 transition-colors"
                        title="Remove association"
                      >
                        <Link2Off className="h-4 w-4" />
                      </button>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-[var(--text-muted)] italic mb-4">No products associated</p>
              )}
 
              {/* Add product dropdown */}
              {unassociatedProducts.length > 0 && (
                <div className="space-y-2">
                  <p className="text-xs font-medium text-[var(--text-secondary)]">Add a product:</p>
                  <div className="max-h-40 overflow-y-auto space-y-1 rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] p-2">
                    {unassociatedProducts.map((product) => (
                      <button
                        key={product.id}
                        onClick={() => handleAssociate(product.id)}
                        className="flex w-full items-center justify-between rounded px-2 py-1.5 text-sm text-[var(--text-primary)] hover:bg-[var(--bg-tertiary)] transition-colors"
                      >
                        <span className="truncate">{product.name}</span>
                        <span className="shrink-0 text-xs text-[var(--text-muted)] font-mono ml-2">{product.sku}</span>
                      </button>
                    ))}
                  </div>
                </div>
              )}
            </section>
          </div>
        ) : null}
      </div>
    </>
  );
}