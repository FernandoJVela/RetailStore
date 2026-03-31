import * as yup from 'yup';
 
export const addStockSchema = yup.object({
  quantity: yup.number().integer('Must be a whole number').positive('Must be positive').required('Quantity is required'),
});
export type AddStockFormData = yup.InferType<typeof addStockSchema>;
 
export const adjustStockSchema = yup.object({
  newQuantity: yup.number().integer('Must be a whole number').min(0, 'Cannot be negative').required('Quantity is required'),
  reason: yup.string().min(3, 'At least 3 characters').max(500).required('Reason is required'),
});
export type AdjustStockFormData = yup.InferType<typeof adjustStockSchema>;
 
export const updateThresholdSchema = yup.object({
  newThreshold: yup.number().integer('Must be a whole number').min(0, 'Cannot be negative').required('Threshold is required'),
});
export type UpdateThresholdFormData = yup.InferType<typeof updateThresholdSchema>;
 
export const createInventorySchema = yup.object({
  productId: yup.string().required('Product is required'),
  initialQuantity: yup.number().integer().min(0).required('Initial quantity is required'),
  reorderThreshold: yup.number().integer().min(0).default(10).required(),
});
export type CreateInventoryFormData = yup.InferType<typeof createInventorySchema>;