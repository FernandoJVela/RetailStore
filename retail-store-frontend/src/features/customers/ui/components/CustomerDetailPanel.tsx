import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslation } from 'react-i18next';
import { X, Mail, MapPin, Pencil, Phone } from 'lucide-react';
import { Button, Input, Badge, Spinner } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import { formatDate } from '@shared/lib/utils';
import {
  useCustomer, useUpdateCustomer, useChangeCustomerEmail,
  useUpdateCustomerAddress,
} from '@features/customers/application/hooks/useCustomersQueries';
import {
  updateCustomerSchema, changeEmailSchema, updateAddressSchema,
  type UpdateCustomerFormData, type ChangeEmailFormData, type UpdateAddressFormData,
} from '@features/customers/application/useCases/customers.validation';
 
interface CustomerDetailPanelProps {
  customerId: string;
  isOpen: boolean;
  onClose: () => void;
}
 
type EditMode = 'none' | 'profile' | 'email' | 'address';
 
export function CustomerDetailPanel({ customerId, isOpen, onClose }: CustomerDetailPanelProps) {
  const { t } = useTranslation();
  const { data: customer, isLoading } = useCustomer(customerId);
  const [editMode, setEditMode] = useState<EditMode>('none');
  const [apiError, setApiError] = useState('');
 
  const updateMutation = useUpdateCustomer();
  const emailMutation = useChangeCustomerEmail();
  const addressMutation = useUpdateCustomerAddress();
 
  // ─── Profile form ─────────────────────────────────────────
  const profileForm = useForm<UpdateCustomerFormData>({
    resolver: yupResolver(updateCustomerSchema),
    values: customer ? { firstName: customer.firstName, lastName: customer.lastName, phone: customer.phone || '' } : undefined,
  });
 
  const handleProfileSave = async (data: UpdateCustomerFormData) => {
    setApiError('');
    try {
      await updateMutation.mutateAsync({ id: customerId, data });
      setEditMode('none');
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  // ─── Email form ───────────────────────────────────────────
  const emailForm = useForm<ChangeEmailFormData>({
    resolver: yupResolver(changeEmailSchema),
    values: customer ? { newEmail: customer.email } : undefined,
  });
 
  const handleEmailSave = async (data: ChangeEmailFormData) => {
    setApiError('');
    try {
      await emailMutation.mutateAsync({ id: customerId, newEmail: data.newEmail });
      setEditMode('none');
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  // ─── Address form ─────────────────────────────────────────
  const addressForm = useForm<UpdateAddressFormData>({
    resolver: yupResolver(updateAddressSchema),
    values: customer?.shippingAddress
      ? { street: customer.shippingAddress.street, city: customer.shippingAddress.city, state: customer.shippingAddress.state, zipCode: customer.shippingAddress.zipCode, country: customer.shippingAddress.country }
      : { street: '', city: '', state: '', zipCode: '', country: 'Colombia' },
  });
 
  const handleAddressSave = async (data: UpdateAddressFormData) => {
    setApiError('');
    try {
      await addressMutation.mutateAsync({ id: customerId, data });
      setEditMode('none');
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  if (!isOpen) return null;
 
  return (
    <>
      {/* Backdrop */}
      <div className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm" onClick={onClose} />
 
      {/* Panel */}
      <div className="fixed inset-y-0 right-0 z-50 w-full max-w-lg overflow-y-auto bg-[var(--bg-secondary)] shadow-2xl">
        {/* Header */}
        <div className="sticky top-0 z-10 flex items-center justify-between border-b border-[var(--border-color)] bg-[var(--bg-secondary)] px-6 py-4">
          <h2 className="text-lg font-semibold text-[var(--text-primary)]">Customer Details</h2>
          <button onClick={onClose} className="rounded-lg p-1.5 text-[var(--text-muted)] hover:bg-[var(--bg-tertiary)]">
            <X className="h-5 w-5" />
          </button>
        </div>
 
        {isLoading ? (
          <Spinner />
        ) : customer ? (
          <div className="p-6 space-y-6">
            {apiError && (
              <div className="rounded-lg bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-800 p-3">
                <p className="text-sm text-red-700 dark:text-red-400">{apiError}</p>
              </div>
            )}
 
            {/* ─── Profile Section ────────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider">Profile</h3>
                <button onClick={() => setEditMode(editMode === 'profile' ? 'none' : 'profile')} className="text-primary-600 hover:text-primary-500">
                  <Pencil className="h-4 w-4" />
                </button>
              </div>
 
              {editMode === 'profile' ? (
                <form onSubmit={profileForm.handleSubmit(handleProfileSave)} className="space-y-3">
                  <div className="grid grid-cols-2 gap-3">
                    <Input label="First Name" error={profileForm.formState.errors.firstName?.message} {...profileForm.register('firstName')} />
                    <Input label="Last Name" error={profileForm.formState.errors.lastName?.message} {...profileForm.register('lastName')} />
                  </div>
                  <Input label="Phone" error={profileForm.formState.errors.phone?.message} {...profileForm.register('phone')} />
                  <div className="flex justify-end gap-2 pt-1">
                    <Button variant="secondary" size="sm" type="button" onClick={() => setEditMode('none')}>{t('common.cancel')}</Button>
                    <Button size="sm" type="submit" loading={updateMutation.isPending}>{t('common.save')}</Button>
                  </div>
                </form>
              ) : (
                <div className="space-y-3">
                  <div className="flex items-center gap-3">
                    <div className="flex h-12 w-12 items-center justify-center rounded-full bg-primary-100 dark:bg-primary-500/15 text-lg font-bold text-primary-700 dark:text-primary-400">
                      {customer.firstName.charAt(0)}{customer.lastName.charAt(0)}
                    </div>
                    <div>
                      <p className="font-medium text-[var(--text-primary)]">{customer.fullName}</p>
                      <Badge variant={customer.isActive ? 'success' : 'danger'}>{customer.statusLabel}</Badge>
                    </div>
                  </div>
                  {customer.phone && (
                    <div className="flex items-center gap-2 text-sm text-[var(--text-secondary)]">
                      <Phone className="h-4 w-4" /> {customer.phone}
                    </div>
                  )}
                  <p className="text-xs text-[var(--text-muted)]">Registered {formatDate(customer.createdAt)}</p>
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
                  <Mail className="h-4 w-4" /> {customer.email}
                </div>
              )}
            </section>
 
            {/* ─── Address Section ────────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider">Shipping Address</h3>
                <button onClick={() => setEditMode(editMode === 'address' ? 'none' : 'address')} className="text-primary-600 hover:text-primary-500">
                  <Pencil className="h-4 w-4" />
                </button>
              </div>
 
              {editMode === 'address' ? (
                <form onSubmit={addressForm.handleSubmit(handleAddressSave)} className="space-y-3">
                  <Input label="Street" error={addressForm.formState.errors.street?.message} {...addressForm.register('street')} />
                  <div className="grid grid-cols-2 gap-3">
                    <Input label="City" error={addressForm.formState.errors.city?.message} {...addressForm.register('city')} />
                    <Input label="State" error={addressForm.formState.errors.state?.message} {...addressForm.register('state')} />
                  </div>
                  <div className="grid grid-cols-2 gap-3">
                    <Input label="ZIP Code" error={addressForm.formState.errors.zipCode?.message} {...addressForm.register('zipCode')} />
                    <Input label="Country" error={addressForm.formState.errors.country?.message} {...addressForm.register('country')} />
                  </div>
                  <div className="flex justify-end gap-2 pt-1">
                    <Button variant="secondary" size="sm" type="button" onClick={() => setEditMode('none')}>{t('common.cancel')}</Button>
                    <Button size="sm" type="submit" loading={addressMutation.isPending}>{t('common.save')}</Button>
                  </div>
                </form>
              ) : customer.hasAddress ? (
                <div className="flex items-start gap-2 text-sm text-[var(--text-secondary)]">
                  <MapPin className="h-4 w-4 mt-0.5 shrink-0" />
                  <span>{customer.shippingAddress!.fullAddress}</span>
                </div>
              ) : (
                <p className="text-sm text-[var(--text-muted)] italic">No address on file</p>
              )}
            </section>
          </div>
        ) : null}
      </div>
    </>
  );
}