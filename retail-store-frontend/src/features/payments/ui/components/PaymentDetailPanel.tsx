import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslation } from 'react-i18next';
import {
  X, CheckCircle, XCircle, AlertTriangle, 
  Clock, CreditCard, RotateCcw
} from 'lucide-react';
import { Button, Input, Badge, Spinner } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import { formatDateTime } from '@shared/lib/utils';
import {
  usePayment, useAuthorizePayment, useCapturePayment,
  useCancelPayment, useRequestRefund, useCompleteRefund,
} from '@features/payments/application/hooks/usePaymentsQueries';
import { paymentStatusVariant, type Refund } from '@features/payments';
import { requestRefundSchema, type RequestRefundFormData } from '@features/payments/application/useCases/payments.validation';
 
interface PaymentDetailPanelProps {
  paymentId: string;
  isOpen: boolean;
  onClose: () => void;
}
 
export function PaymentDetailPanel({ paymentId, isOpen, onClose }: PaymentDetailPanelProps) {
  const { t } = useTranslation();
  const { data: payment, isLoading } = usePayment(paymentId);
  const [showRefundForm, setShowRefundForm] = useState(false);
  const [apiError, setApiError] = useState('');
 
  const authorizeMutation = useAuthorizePayment();
  const captureMutation = useCapturePayment();
  const cancelMutation = useCancelPayment();
  const refundMutation = useRequestRefund();
  const completeRefundMutation = useCompleteRefund();
 
  const refundForm = useForm<RequestRefundFormData>({
    resolver: yupResolver(requestRefundSchema),
    defaultValues: { amount: 0, reason: '' },
  });
 
  const handleAction = async (action: 'authorize' | 'capture' | 'cancel') => {
    setApiError('');
    try {
      if (action === 'authorize') await authorizeMutation.mutateAsync({ id: paymentId });
      if (action === 'capture') await captureMutation.mutateAsync(paymentId);
      if (action === 'cancel') await cancelMutation.mutateAsync({ id: paymentId });
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  const handleRefund = async (data: RequestRefundFormData) => {
    setApiError('');
    try {
      await refundMutation.mutateAsync({ paymentId, data });
      refundForm.reset();
      setShowRefundForm(false);
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  const handleCompleteRefund = async (refundId: string) => {
    setApiError('');
    try {
      await completeRefundMutation.mutateAsync({ paymentId, refundId });
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  if (!isOpen) return null;
 
  // Timeline events from payment data
  const timeline: { label: string; date: Date | null; icon: typeof Clock; color: string }[] = payment ? [
    { label: 'Created', date: payment.createdAt, icon: Clock, color: 'text-[var(--text-muted)]' },
    { label: 'Authorized', date: payment.authorizedAt, icon: CheckCircle, color: 'text-blue-500' },
    { label: 'Captured', date: payment.capturedAt, icon: CheckCircle, color: 'text-emerald-500' },
    { label: 'Failed', date: payment.failedAt, icon: XCircle, color: 'text-red-500' },
    { label: 'Refunded', date: payment.refundedAt, icon: RotateCcw, color: 'text-amber-500' },
    { label: 'Cancelled', date: payment.cancelledAt, icon: XCircle, color: 'text-red-500' },
  ].filter((e) => e.date !== null) : [];
 
  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm" onClick={onClose} />
      <div className="fixed inset-y-0 right-0 z-50 w-full max-w-lg overflow-y-auto bg-[var(--bg-secondary)] shadow-2xl">
        <div className="sticky top-0 z-10 flex items-center justify-between border-b border-[var(--border-color)] bg-[var(--bg-secondary)] px-6 py-4">
          <h2 className="text-lg font-semibold text-[var(--text-primary)]">Payment Details</h2>
          <button onClick={onClose} className="rounded-lg p-1.5 text-[var(--text-muted)] hover:bg-[var(--bg-tertiary)]">
            <X className="h-5 w-5" />
          </button>
        </div>
 
        {isLoading ? (
          <Spinner />
        ) : payment ? (
          <div className="p-6 space-y-6">
            {apiError && (
              <div className="rounded-lg bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-800 p-3">
                <p className="text-sm text-red-700 dark:text-red-400">{apiError}</p>
              </div>
            )}
 
            {/* ─── Amount + Status Header ────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <div className="flex items-center justify-between mb-3">
                <Badge variant={paymentStatusVariant(payment.status)}>{payment.status}</Badge>
                {payment.gatewayName && (
                  <span className="text-xs text-[var(--text-muted)]">via {payment.gatewayName}</span>
                )}
              </div>
              <p className="text-3xl font-bold text-[var(--text-primary)]">{payment.formattedAmount}</p>
              {payment.hasRefunds && (
                <p className="mt-1 text-sm text-amber-600 dark:text-amber-400">
                  Net: {payment.formattedNet} ({payment.totalRefunded.toFixed(2)} refunded)
                </p>
              )}
              <div className="mt-3 flex items-center gap-2 text-sm text-[var(--text-secondary)]">
                <CreditCard className="h-4 w-4" /> {payment.methodLabel}
              </div>
              {payment.gatewayTransactionId && (
                <p className="mt-1 text-xs text-[var(--text-muted)] font-mono">{payment.gatewayTransactionId}</p>
              )}
              {payment.failureReason && (
                <div className="mt-3 flex items-start gap-2 rounded-lg bg-red-50 dark:bg-red-500/10 p-3">
                  <AlertTriangle className="h-4 w-4 text-red-500 mt-0.5 shrink-0" />
                  <p className="text-sm text-red-700 dark:text-red-400">{payment.failureReason}</p>
                </div>
              )}
            </section>
 
            {/* ─── Lifecycle Actions ─────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3">Actions</h3>
              <div className="flex flex-wrap gap-2">
                {payment.status === 'Pending' && (
                  <Button size="sm" variant="primary" onClick={() => handleAction('authorize')} loading={authorizeMutation.isPending}>
                    Authorize
                  </Button>
                )}
                {payment.status === 'Authorized' && (
                  <Button size="sm" variant="primary" onClick={() => handleAction('capture')} loading={captureMutation.isPending}>
                    Capture
                  </Button>
                )}
                {(payment.status === 'Captured' || payment.status === 'PartialRefund') && (
                  <Button size="sm" variant="outline" onClick={() => setShowRefundForm(!showRefundForm)}>
                    <RotateCcw className="h-4 w-4" /> Request Refund
                  </Button>
                )}
                {(payment.status === 'Pending' || payment.status === 'Authorized') && (
                  <Button size="sm" variant="danger" onClick={() => handleAction('cancel')} loading={cancelMutation.isPending}>
                    Cancel
                  </Button>
                )}
                {payment.status === 'Captured' && !showRefundForm && payment.refunds.length === 0 && (
                  <p className="text-xs text-[var(--text-muted)] self-center ml-2">Payment captured successfully</p>
                )}
              </div>
 
              {/* Refund form */}
              {showRefundForm && (
                <form onSubmit={refundForm.handleSubmit(handleRefund)} className="mt-4 space-y-3 rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] p-4">
                  <p className="text-sm font-medium text-[var(--text-primary)]">Request Refund (max: {payment.netAmount.toFixed(2)})</p>
                  <div className="grid grid-cols-2 gap-3">
                    <Input label="Amount" type="number" step="0.01" max={payment.netAmount} error={refundForm.formState.errors.amount?.message} {...refundForm.register('amount', { valueAsNumber: true })} />
                    <div />
                  </div>
                  <div className="space-y-1.5">
                    <label className="block text-sm font-medium text-[var(--text-secondary)]">Reason</label>
                    <textarea
                      rows={2}
                      placeholder="Customer request, defective item..."
                      className="w-full rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] px-3.5 py-2.5 text-sm text-[var(--text-primary)] placeholder:text-[var(--text-muted)] focus:border-primary-500 focus:outline-none resize-none"
                      {...refundForm.register('reason')}
                    />
                    {refundForm.formState.errors.reason && (
                      <p className="text-xs text-danger">{refundForm.formState.errors.reason.message}</p>
                    )}
                  </div>
                  <div className="flex justify-end gap-2">
                    <Button variant="secondary" size="sm" type="button" onClick={() => setShowRefundForm(false)}>{t('common.cancel')}</Button>
                    <Button size="sm" type="submit" loading={refundMutation.isPending}>Submit Refund</Button>
                  </div>
                </form>
              )}
            </section>
 
            {/* ─── Timeline ──────────────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3">Timeline</h3>
              <div className="space-y-3">
                {timeline.map((event, idx) => (
                  <div key={idx} className="flex items-center gap-3">
                    <event.icon className={`h-4 w-4 shrink-0 ${event.color}`} />
                    <div className="flex-1">
                      <span className="text-sm font-medium text-[var(--text-primary)]">{event.label}</span>
                    </div>
                    <span className="text-xs text-[var(--text-muted)] tabular-nums">
                      {event.date ? formatDateTime(event.date) : ''}
                    </span>
                  </div>
                ))}
              </div>
            </section>
 
            {/* ─── Refunds History ────────────────────────── */}
            {payment.refunds.length > 0 && (
              <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
                <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3">
                  Refunds ({payment.refunds.length})
                </h3>
                <div className="space-y-3">
                  {payment.refunds.map((refund) => (
                    <RefundCard
                      key={refund.id}
                      refund={refund}
                      paymentId={payment.id}
                      onComplete={handleCompleteRefund}
                    />
                  ))}
                </div>
              </section>
            )}
          </div>
        ) : null}
      </div>
    </>
  );
}
 
function RefundCard({ refund, paymentId, onComplete }: {
  refund: Refund; paymentId: string; onComplete: (refundId: string) => void;
}) {
  const statusVariant = refund.status === 'Completed' ? 'success'
    : refund.status === 'Failed' ? 'danger'
    : refund.status === 'Processing' ? 'info' : 'warning';
 
  return (
    <div className="flex items-center justify-between rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] p-3">
      <div className="min-w-0">
        <div className="flex items-center gap-2">
          <span className="font-semibold text-[var(--text-primary)] tabular-nums">{refund.formattedAmount}</span>
          <Badge variant={statusVariant}>{refund.status}</Badge>
        </div>
        <p className="mt-0.5 text-xs text-[var(--text-secondary)] truncate">{refund.reason}</p>
        {refund.processedAt && (
          <p className="text-xs text-[var(--text-muted)]">Processed {formatDateTime(refund.processedAt)}</p>
        )}
      </div>
      {refund.status === 'Pending' && (
        <Button size="sm" variant="primary" onClick={() => onComplete(refund.id)}>
          Complete
        </Button>
      )}
    </div>
  );
}