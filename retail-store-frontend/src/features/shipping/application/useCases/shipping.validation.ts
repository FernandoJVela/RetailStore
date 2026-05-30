import * as yup from 'yup';
 
export const assignCarrierSchema = yup.object({
  carrier: yup.string().max(100).required('Carrier is required'),
  trackingNumber: yup.string().max(200).required('Tracking number is required'),
  estimatedDelivery: yup.string().default(''),
});
export type AssignCarrierFormData = yup.InferType<typeof assignCarrierSchema>;
 
export const shippingCostSchema = yup.object({
  cost: yup.number().min(0, 'Cannot be negative').required('Cost is required'),
  currency: yup.string().length(3).default('USD').required(),
});
export type ShippingCostFormData = yup.InferType<typeof shippingCostSchema>;
 
export const reasonSchema = yup.object({
  reason: yup.string().min(3, 'At least 3 characters').max(500).required('Reason is required'),
});
export type ReasonFormData = yup.InferType<typeof reasonSchema>;