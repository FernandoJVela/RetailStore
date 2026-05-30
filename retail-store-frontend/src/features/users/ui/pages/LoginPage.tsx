import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Store } from 'lucide-react';
import { Button, Input, Alert } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import { useLogin, loginSchema, type LoginFormData } from '@features/users';
import { useState } from 'react';
 
export function LoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const loginMutation = useLogin();
  const [apiError, setApiError] = useState('');
 
  const { register, handleSubmit, formState: { errors } } = useForm<LoginFormData>({
    resolver: yupResolver(loginSchema),
  });
 
  const onSubmit = async (data: LoginFormData) => {
    setApiError('');
    try {
      await loginMutation.mutateAsync(data);
      navigate('/');
    } catch (err) {
      setApiError(getApiErrorMessage(err));
    }
  };
 
  return (
    <div className="flex min-h-screen">
      {/* Left panel - branding */}
      <div className="hidden lg:flex lg:w-1/2 bg-primary-950 flex-col justify-between p-12">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary-600">
            <Store className="h-6 w-6 text-white" />
          </div>
          <span className="text-2xl font-bold text-white tracking-tight">{t('common.appName')}</span>
        </div>
        <div>
          <h1 className="text-4xl font-bold text-white leading-tight" style={{ whiteSpace: 'pre-line' }}>
            {t('auth.brandingTitle')}
          </h1>
          <p className="mt-4 text-lg text-primary-300 max-w-md">
            {t('auth.brandingDescription')}
          </p>
        </div>
        <p className="text-sm text-primary-400">{t('auth.copyright')}</p>
      </div>
 
      {/* Right panel - login form */}
      <div className="flex flex-1 items-center justify-center px-8 bg-[var(--bg-primary)]">
        <div className="w-full max-w-md">
          <div className="mb-8">
            <h2 className="text-3xl font-bold text-[var(--text-primary)]">{t('auth.loginTitle')}</h2>
            <p className="mt-2 text-[var(--text-secondary)]">{t('auth.loginSubtitle')}</p>
          </div>
 
          {apiError && <Alert message={apiError} className="mb-6" />}
 
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
            <Input
              label={t('auth.email')}
              type="email"
              placeholder="admin@retailstore.com"
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
            <Button type="submit" className="w-full" size="lg" loading={loginMutation.isPending}>
              {t('auth.login')}
            </Button>
          </form>
 
          <p className="mt-6 text-center text-sm text-[var(--text-secondary)]">
            {t('auth.noAccount')}{' '}
            <Link to="/register" className="font-medium text-primary-600 hover:text-primary-500">
              {t('auth.register')}
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}