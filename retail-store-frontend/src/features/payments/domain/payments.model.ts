export type PaymentStatus = 'Pending' | 'Authorized' | 'Captured' | 'Failed' | 'Refunded' | 'PartialRefund' | 'Cancelled' | 'Expired';
export type RefundStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed';
 
export interface Payment {
  id: string;
  orderId: string;
  customerId: string;
  amount: number;
  currency: string;
  formattedAmount: string;
  method: string;
  methodLabel: string;       // Computed: "Credit Card •••• 4532"
  methodDetail: string | null;
  status: PaymentStatus;
  gatewayName: string | null;
  gatewayTransactionId: string | null;
  totalRefunded: number;
  netAmount: number;
  formattedNet: string;
  hasRefunds: boolean;
  capturedAt: Date | null;
  createdAt: Date;
}
 
export interface PaymentDetail extends Payment {
  authorizedAt: Date | null;
  failedAt: Date | null;
  refundedAt: Date | null;
  cancelledAt: Date | null;
  failureReason: string | null;
  notes: string | null;
  refunds: Refund[];
}
 
export interface Refund {
  id: string;
  amount: number;
  currency: string;
  formattedAmount: string;
  reason: string;
  status: RefundStatus;
  processedAt: Date | null;
}
 
export interface RequestRefundData {
  amount: number;
  reason: string;
}
 
/** Payment methods (matches backend PaymentMethod enum) */
export const PAYMENT_METHODS = [
  'CreditCard', 'DebitCard', 'BankTransfer', 'Cash', 'DigitalWallet', 'PSE',
] as const;
 
export const PAYMENT_METHOD_LABELS: Record<string, string> = {
  CreditCard: 'Credit Card',
  DebitCard: 'Debit Card',
  BankTransfer: 'Bank Transfer',
  Cash: 'Cash',
  DigitalWallet: 'Digital Wallet',
  PSE: 'PSE',
};
 
/** Badge variant per payment status */
export function paymentStatusVariant(status: PaymentStatus): 'success' | 'warning' | 'danger' | 'info' | 'default' {
  switch (status) {
    case 'Captured': return 'success';
    case 'Authorized': return 'info';
    case 'Pending': return 'warning';
    case 'PartialRefund': return 'warning';
    case 'Failed': return 'danger';
    case 'Cancelled': return 'danger';
    case 'Expired': return 'danger';
    case 'Refunded': return 'default';
  }
}