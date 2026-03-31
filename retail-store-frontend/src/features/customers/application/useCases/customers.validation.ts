import * as yup from 'yup';
 
export const registerCustomerSchema = yup.object({
  firstName: yup.string().min(2, 'At least 2 characters').max(100).required('First name is required'),
  lastName: yup.string().min(2, 'At least 2 characters').max(100).required('Last name is required'),
  email: yup.string().email('Invalid email').max(256).required('Email is required'),
  phone: yup.string().max(20).default('').optional(),
  includeAddress: yup.boolean().default(false),
  street: yup.string().max(300).default('').when('includeAddress', { is: true, then: (s) => s.required('Street is required') }),
  city: yup.string().max(100).default('').when('includeAddress', { is: true, then: (s) => s.required('City is required') }),
  state: yup.string().max(100).default('').optional(),
  zipCode: yup.string().max(20).default('').optional(),
  country: yup.string().max(100).default('').when('includeAddress', { is: true, then: (s) => s.required('Country is required') }),
}).required();
export type RegisterCustomerFormData = yup.InferType<typeof registerCustomerSchema>;
 
export const updateCustomerSchema = yup.object({
  firstName: yup.string().min(2).max(100).required('First name is required'),
  lastName: yup.string().min(2).max(100).required('Last name is required'),
  phone: yup.string().max(20).default('').optional(),
});
export type UpdateCustomerFormData = yup.InferType<typeof updateCustomerSchema>;
 
export const changeEmailSchema = yup.object({
  newEmail: yup.string().email('Invalid email').max(256).required('Email is required'),
});
export type ChangeEmailFormData = yup.InferType<typeof changeEmailSchema>;
 
export const updateAddressSchema = yup.object({
  street: yup.string().max(300).required('Street is required'),
  city: yup.string().max(100).required('City is required'),
  state: yup.string().max(100).optional().default(''),
  zipCode: yup.string().max(20).optional().default(''),
  country: yup.string().max(100).required('Country is required'),
});
export type UpdateAddressFormData = yup.InferType<typeof updateAddressSchema>;