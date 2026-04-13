import * as yup from 'yup';
 
export const requestRefundSchema = yup.object({
  amount: yup.number().positive('Must be positive').required('Amount is required'),
  reason: yup.string().min(3, 'At least 3 characters').max(500).required('Reason is required'),
});
export type RequestRefundFormData = yup.InferType<typeof requestRefundSchema>;
 
export const failReasonSchema = yup.object({
  reason: yup.string().min(3, 'At least 3 characters').max(500).required('Reason is required'),
});
export type FailReasonFormData = yup.InferType<typeof failReasonSchema>;