import * as yup from 'yup';
 
export const createProductSchema = yup.object({
  name: yup.string().max(200).required('Product name is required'),
  sku: yup.string().max(50).required('SKU is required'),
  price: yup.number().positive('Price must be positive').required('Price is required'),
  currency: yup.string().length(3).default('USD').required(),
  category: yup.string().max(100).required('Category is required'),
  // Force this to be a string and use an empty string default 
  // to avoid the 'undefined' headache entirely.
  description: yup.string().max(2000).default('').optional(),
}).required();

export type CreateProductFormData = yup.InferType<typeof createProductSchema>;
 
export const updateDetailsSchema = yup.object({
  name: yup.string().max(200).required('Product name is required'),
  category: yup.string().max(100).required('Category is required'),
  description: yup.string().max(2000).default('').optional(),
}).required();

export type UpdateDetailsFormData = yup.InferType<typeof updateDetailsSchema>;
 
export const updatePriceSchema = yup.object({
  price: yup.number().positive('Price must be positive').required('Price is required'),
  currency: yup.string().length(3).default('USD').required(),
});
export type UpdatePriceFormData = yup.InferType<typeof updatePriceSchema>;