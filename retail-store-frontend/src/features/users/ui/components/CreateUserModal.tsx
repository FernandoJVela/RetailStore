import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslation } from 'react-i18next';
import { Modal, Button, Input, Alert } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import { useRegister } from '@features/users/application/hooks/useUsersQueries';
import { registerSchema, type RegisterFormData } from '@features/users/application/useCases/auth.validation';
import { useState } from 'react';

interface CreateUserModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CreateUserModal({ isOpen, onClose }: CreateUserModalProps) {
  const { t } = useTranslation();
  const registerMutation = useRegister();
  const [apiError, setApiError] = useState('');

  const { register, handleSubmit, reset, formState: { errors } } = useForm<RegisterFormData>({
    resolver: yupResolver(registerSchema),
  });

  const onSubmit = async (data: RegisterFormData) => {
    setApiError('');
    try {
      await registerMutation.mutateAsync({
        username: data.username,
        email: data.email,
        password: data.password,
        confirmPassword: data.confirmPassword,
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
    <Modal isOpen={isOpen} onClose={handleClose} title={t('users.createUser')} size="md">
      {apiError && <Alert message={apiError} className="mb-4" />}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <Input
          label={t('users.username')}
          placeholder="john.doe"
          error={errors.username?.message}
          {...register('username')}
        />

        <Input
          label={t('auth.email')}
          type="email"
          placeholder="john.doe@company.com"
          error={errors.email?.message}
          {...register('email')}
        />

        <Input
          label={t('auth.password')}
          type="password"
          placeholder="••••••••"
          error={errors.password?.message}
          {...register('password')}
        />

        <Input
          label={t('auth.confirmPassword')}
          type="password"
          placeholder="••••••••"
          error={errors.confirmPassword?.message}
          {...register('confirmPassword')}
        />

        <div className="flex justify-end gap-3 pt-2">
          <Button variant="secondary" type="button" onClick={handleClose}>
            {t('common.cancel')}
          </Button>
          <Button type="submit" loading={registerMutation.isPending}>
            {t('users.createUser')}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
