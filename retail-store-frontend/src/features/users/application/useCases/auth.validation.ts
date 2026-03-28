import * as yup from 'yup';
import i18n from '@shared/i18n';
 
const t = (key: string) => i18n.t(key);
 
export const loginSchema = yup.object({
  email: yup
    .string()
    .email(t('auth.emailRequired'))
    .required(t('auth.emailRequired')),
  password: yup
    .string()
    .min(8, t('auth.passwordMin'))
    .required(t('auth.passwordRequired')),
});
 
export const registerSchema = yup.object({
  username: yup
    .string()
    .min(3, t('auth.usernameMin'))
    .required(t('auth.usernameRequired')),
  email: yup
    .string()
    .email(t('auth.emailRequired'))
    .required(t('auth.emailRequired')),
  password: yup
    .string()
    .min(8, t('auth.passwordMin'))
    .required(t('auth.passwordRequired')),
  confirmPassword: yup
    .string()
    .oneOf([yup.ref('password')], t('auth.passwordsMustMatch'))
    .required(t('auth.passwordRequired')),
});
 
export type LoginFormData = yup.InferType<typeof loginSchema>;
export type RegisterFormData = yup.InferType<typeof registerSchema>;