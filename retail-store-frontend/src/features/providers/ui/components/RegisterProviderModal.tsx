import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslation } from 'react-i18next';
import { Modal, Button, Input } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import { 
  useRegisterProvider, 
  registerProviderSchema, 
  type RegisterProviderFormData } from '@features/providers';
import { useState } from 'react';
 
interface RegisterProviderModalProps {
  isOpen: boolean;
  onClose: () => void;
}
 
export function RegisterProviderModal({ isOpen, onClose }: RegisterProviderModalProps) {
  const { t } = useTranslation();
  const registerMutation = useRegisterProvider();
  const [apiError, setApiError] = useState('');
 
  const { register, handleSubmit, reset, formState: { errors } } = useForm<RegisterProviderFormData>({
    resolver: yupResolver(registerProviderSchema),
  });
 
  const onSubmit = async (data: RegisterProviderFormData) => {
    setApiError('');
    try {
      await registerMutation.mutateAsync(data);
      reset();
      onClose();
    } catch (err) {
      setApiError(getApiErrorMessage(err));
    }
  };
 
  const handleClose = () => { reset(); setApiError(''); onClose(); };
 
  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Register Provider" size="md">
      {apiError && (
        <div className="mb-4 rounded-lg bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-800 p-3">
          <p className="text-sm text-red-700 dark:text-red-400">{apiError}</p>
        </div>
      )}
 
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <Input label="Company Name" placeholder="Distribuidora Nacional S.A." error={errors.companyName?.message} {...register('companyName')} />
        <Input label="Contact Name" placeholder="Carlos Rodríguez" error={errors.contactName?.message} {...register('contactName')} />
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Input label="Email" type="email" placeholder="contacto@empresa.co" error={errors.email?.message} {...register('email')} />
          <Input label="Phone" placeholder="+57 601 555 0101" error={errors.phone?.message} {...register('phone')} />
        </div>
 
        <div className="flex justify-end gap-3 pt-2">
          <Button variant="secondary" type="button" onClick={handleClose}>{t('common.cancel')}</Button>
          <Button type="submit" loading={registerMutation.isPending}>{t('common.save')}</Button>
        </div>
      </form>
    </Modal>
  );
}