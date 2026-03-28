import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Store } from 'lucide-react';
import { Button, Input } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import { useRegister, registerSchema, type RegisterFormData } from '@features/users';
import { useState } from 'react';
 
export function RegisterPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const registerMutation = useRegister();
  const [apiError, setApiError] = useState('');
 
  const { register, handleSubmit, formState: { errors } } = useForm<RegisterFormData>({
    resolver: yupResolver(registerSchema),
  });
 
  const onSubmit = async (data: RegisterFormData) => {
    setApiError('');
    try {
      await registerMutation.mutateAsync(data);
      navigate('/login');
    } catch (err) {
      setApiError(getApiErrorMessage(err));
    }
  };
 
  return (
    <div className="flex min-h-screen">
      {/* Left panel */}
      <div className="hidden lg:flex lg:w-1/2 bg-primary-950 flex-col justify-between p-12">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary-600">
            <Store className="h-6 w-6 text-white" />
          </div>
          <span className="text-2xl font-bold text-white tracking-tight">RetailStore</span>
        </div>
        <div>
          <h1 className="text-4xl font-bold text-white leading-tight">
            Join RetailStore<br />Management Platform
          </h1>
          <p className="mt-4 text-lg text-primary-300 max-w-md">
            Create your account to access the full retail management suite.
          </p>
        </div>
        <p className="text-sm text-primary-400">© 2026 RetailStore. Built with .NET 10 + React.</p>
      </div>
 
      {/* Right panel */}
      <div className="flex flex-1 items-center justify-center px-8 bg-[var(--bg-primary)]">
        <div className="w-full max-w-md">
          <div className="mb-8">
            <h2 className="text-3xl font-bold text-[var(--text-primary)]">{t('auth.registerTitle')}</h2>
            <p className="mt-2 text-[var(--text-secondary)]">{t('auth.registerSubtitle')}</p>
          </div>
 
          {apiError && (
            <div className="mb-6 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 p-4">
              <p className="text-sm text-red-700 dark:text-red-400">{apiError}</p>
            </div>
          )}
 
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
            <Input
              label={t('auth.username')}
              placeholder="johndoe"
              error={errors.username?.message}
              {...register('username')}
            />
            <Input
              label={t('auth.email')}
              type="email"
              placeholder="john@retailstore.com"
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
            <Button type="submit" className="w-full" size="lg" loading={registerMutation.isPending}>
              {t('auth.register')}
            </Button>
          </form>
 
          <p className="mt-6 text-center text-sm text-[var(--text-secondary)]">
            {t('auth.hasAccount')}{' '}
            <Link to="/login" className="font-medium text-primary-600 hover:text-primary-500">
              {t('auth.login')}
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}