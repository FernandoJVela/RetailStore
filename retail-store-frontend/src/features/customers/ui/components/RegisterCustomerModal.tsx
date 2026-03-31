import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslation } from 'react-i18next';
import { Modal, Button, Input } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import { useRegisterCustomer } from '@features/customers/application/hooks/useCustomersQueries';
import { registerCustomerSchema, type RegisterCustomerFormData } from '@features/customers/application/useCases/customers.validation';
import { useState } from 'react';
 
interface RegisterCustomerModalProps {
  isOpen: boolean;
  onClose: () => void;
}
 
export function RegisterCustomerModal({ isOpen, onClose }: RegisterCustomerModalProps) {
  const { t } = useTranslation();
  const registerMutation = useRegisterCustomer();
  const [apiError, setApiError] = useState('');
 
  const { register, handleSubmit, watch, reset, formState: { errors } } = useForm<RegisterCustomerFormData>({
    resolver: yupResolver(registerCustomerSchema),
    defaultValues: { includeAddress: false },
  });
 
  const includeAddress = watch('includeAddress');
 
  const onSubmit = async (formData: RegisterCustomerFormData) => {
    setApiError('');
    try {
      await registerMutation.mutateAsync({
        firstName: formData.firstName,
        lastName: formData.lastName,
        email: formData.email,
        phone: formData.phone || undefined,
        shippingAddress: formData.includeAddress
          ? {
              street: formData.street!,
              city: formData.city!,
              state: formData.state || '',
              zipCode: formData.zipCode || '',
              country: formData.country!,
            }
          : undefined,
      });
      reset();
      onClose();
    } catch (err) {
      setApiError(getApiErrorMessage(err));
    }
  };
 
  const handleClose = () => {
    reset();
    setApiError('');
    onClose();
  };
 
  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Register Customer" size="lg">
      {apiError && (
        <div className="mb-4 rounded-lg bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-800 p-3">
          <p className="text-sm text-red-700 dark:text-red-400">{apiError}</p>
        </div>
      )}
 
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {/* Name row */}
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Input label="First Name" error={errors.firstName?.message} {...register('firstName')} />
          <Input label="Last Name" error={errors.lastName?.message} {...register('lastName')} />
        </div>
 
        {/* Contact */}
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Input label="Email" type="email" error={errors.email?.message} {...register('email')} />
          <Input label="Phone" placeholder="+57 310 555 0101" error={errors.phone?.message} {...register('phone')} />
        </div>
 
        {/* Address toggle */}
        <label className="flex items-center gap-2 text-sm cursor-pointer">
          <input
            type="checkbox"
            className="h-4 w-4 rounded border-[var(--border-color)] text-primary-600 focus:ring-primary-500"
            {...register('includeAddress')}
          />
          <span className="text-[var(--text-secondary)]">Include shipping address</span>
        </label>
 
        {/* Address fields (conditional) */}
        {includeAddress && (
          <div className="space-y-4 rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-4">
            <Input label="Street" placeholder="Calle 100 #15-20" error={errors.street?.message} {...register('street')} />
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <Input label="City" placeholder="Bogotá" error={errors.city?.message} {...register('city')} />
              <Input label="State" placeholder="Cundinamarca" error={errors.state?.message} {...register('state')} />
            </div>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <Input label="ZIP Code" placeholder="110111" error={errors.zipCode?.message} {...register('zipCode')} />
              <Input label="Country" placeholder="Colombia" error={errors.country?.message} {...register('country')} />
            </div>
          </div>
        )}
 
        {/* Actions */}
        <div className="flex justify-end gap-3 pt-2">
          <Button variant="secondary" type="button" onClick={handleClose}>{t('common.cancel')}</Button>
          <Button type="submit" loading={registerMutation.isPending}>{t('common.save')}</Button>
        </div>
      </form>
    </Modal>
  );
}