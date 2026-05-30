import * as yup from 'yup';
 
export const createOrderSchema = yup.object({
  customerId: yup.string().required('Customer is required'),
});
export type CreateOrderFormData = yup.InferType<typeof createOrderSchema>;
 
export const addItemSchema = yup.object({
  productId: yup.string().required('Product is required'),
  quantity: yup.number().integer().min(1, 'At least 1').required('Quantity is required'),
});
export type AddItemFormData = yup.InferType<typeof addItemSchema>;
 
export const cancelReasonSchema = yup.object({
  reason: yup.string().min(3, 'At least 3 characters').max(500).required('Reason is required'),
});
export type CancelReasonFormData = yup.InferType<typeof cancelReasonSchema>;