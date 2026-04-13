import * as yup from 'yup';
 
export const registerProviderSchema = yup.object({
  companyName: yup.string().min(2, 'At least 2 characters').max(200).required('Company name is required'),
  contactName: yup.string().min(2, 'At least 2 characters').max(200).required('Contact name is required'),
  email: yup.string().email('Invalid email').max(256).required('Email is required'),
  phone: yup.string().max(20).default('').optional(),
});
export type RegisterProviderFormData = yup.InferType<typeof registerProviderSchema>;
 
export const updateProviderSchema = yup.object({
  companyName: yup.string().min(2).max(200).required('Company name is required'),
  contactName: yup.string().min(2).max(200).required('Contact name is required'),
  phone: yup.string().max(20).default('').optional(),
});
export type UpdateProviderFormData = yup.InferType<typeof updateProviderSchema>;
 
export const changeEmailSchema = yup.object({
  newEmail: yup.string().email('Invalid email').max(256).required('Email is required'),
});
export type ChangeEmailFormData = yup.InferType<typeof changeEmailSchema>;