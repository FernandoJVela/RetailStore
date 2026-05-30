import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslation } from 'react-i18next';
import { CheckCircle, XCircle, Package, DollarSign, Trash2 } from 'lucide-react';
import { Button, Badge, Spinner, SlidePanel, Textarea, Alert } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import { formatDate, formatDateTime } from '@shared/lib/utils';
import { cn } from '@shared/lib/utils';
import {
  useOrder, useConfirmOrder, useCompleteOrder,
  useCancelOrder, useRemoveOrderItem,
} from '@features/orders/application/hooks/useOrdersQueries';
import { orderStatusVariant, ORDER_STATUS_STEPS } from '@features/orders';
import type { OrderStatus } from '@features/orders';
import { cancelReasonSchema, type CancelReasonFormData } from '@features/orders/application/useCases/orders.validation';
 
interface OrderDetailPanelProps {
  orderId: string;
  isOpen: boolean;
  onClose: () => void;
}
 
export function OrderDetailPanel({ orderId, isOpen, onClose }: OrderDetailPanelProps) {
  const { t } = useTranslation();
  const { data: order, isLoading } = useOrder(orderId);
  const [showCancelForm, setShowCancelForm] = useState(false);
  const [apiError, setApiError] = useState('');
 
  const confirmMut = useConfirmOrder();
  const completeMut = useCompleteOrder();
  const cancelMut = useCancelOrder();
  const removeItemMut = useRemoveOrderItem();
 
  const cancelForm = useForm<CancelReasonFormData>({
    resolver: yupResolver(cancelReasonSchema),
    defaultValues: { reason: '' },
  });
 
  const handleConfirm = async () => {
    setApiError('');
    try { await confirmMut.mutateAsync(orderId); } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  const handleComplete = async () => {
    setApiError('');
    try { await completeMut.mutateAsync(orderId); } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  const handleCancel = async (data: CancelReasonFormData) => {
    setApiError('');
    try {
      await cancelMut.mutateAsync({ id: orderId, reason: data.reason });
      setShowCancelForm(false);
      cancelForm.reset();
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  const handleRemoveItem = async (productId: string) => {
    setApiError('');
    try { await removeItemMut.mutateAsync({ orderId, productId }); } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  const currentStepIdx = order ? ORDER_STATUS_STEPS.indexOf(order.status as OrderStatus) : -1;
  const isCancelled = order?.status === 'Cancelled';
  const isDraft = order?.status === 'Draft';

  return (
    <SlidePanel isOpen={isOpen} onClose={onClose} title="Order Details">
      {isLoading ? (
        <Spinner />
      ) : order ? (
        <div className="p-6 space-y-6">
            {apiError && <Alert message={apiError} />}
 
            {/* ─── Status + Total Header ─────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <div className="flex items-center justify-between mb-3">
                <Badge variant={orderStatusVariant(order.status)}>{order.status}</Badge>
                <span className="text-xs text-[var(--text-muted)]">{formatDate(order.orderDate)}</span>
              </div>
              <p className="text-3xl font-bold text-[var(--text-primary)]">{order.formattedTotal}</p>
              <p className="mt-1 text-sm text-[var(--text-secondary)]">{order.itemCount} {order.itemCount === 1 ? 'item' : 'items'}</p>
 
              {/* Progress bar */}
              {!isCancelled && (
                <div className="mt-4">
                  <div className="flex items-center gap-1">
                    {ORDER_STATUS_STEPS.map((step, idx) => (
                      <div key={step} className="flex-1">
                        <div className={cn(
                          'h-2 rounded-full transition-colors',
                          idx <= currentStepIdx ? 'bg-primary-600' : 'bg-[var(--bg-tertiary)]'
                        )} />
                      </div>
                    ))}
                  </div>
                  <div className="flex justify-between mt-1 text-xs text-[var(--text-muted)]">
                    {ORDER_STATUS_STEPS.map((step) => (
                      <span key={step}>{step}</span>
                    ))}
                  </div>
                </div>
              )}
            </section>
 
            {/* ─── Line Items ────────────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3 flex items-center gap-2">
                <Package className="h-4 w-4" /> Line Items ({order.items.length})
              </h3>
              <div className="space-y-2">
                {order.items.map((item) => (
                  <div key={item.id} className="flex items-center justify-between rounded-lg bg-[var(--bg-secondary)] border border-[var(--border-color)] px-4 py-3">
                    <div className="min-w-0 flex-1">
                      <p className="text-sm font-medium text-[var(--text-primary)] truncate">
                        Product {item.productId.substring(0, 8)}
                      </p>
                      <p className="text-xs text-[var(--text-muted)]">
                        {item.formattedPrice} × {item.quantity}
                      </p>
                    </div>
                    <div className="flex items-center gap-3 shrink-0 ml-4">
                      <span className="text-sm font-semibold text-[var(--text-primary)] tabular-nums">
                        {item.formattedSubtotal}
                      </span>
                      {isDraft && (
                        <button
                          onClick={() => handleRemoveItem(item.productId)}
                          className="rounded p-1 text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
              {/* Total line */}
              <div className="flex items-center justify-between mt-3 pt-3 border-t border-[var(--border-color)]">
                <span className="text-sm font-medium text-[var(--text-secondary)]">Total</span>
                <span className="text-lg font-bold text-[var(--text-primary)]">{order.formattedTotal}</span>
              </div>
            </section>
 
            {/* ─── Actions ───────────────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3">Actions</h3>
              <div className="flex flex-wrap gap-2">
                {(order.status === 'Draft' || order.status === 'Pending') && (
                  <Button size="sm" onClick={handleConfirm} loading={confirmMut.isPending}>
                    <CheckCircle className="h-4 w-4" /> Confirm Order
                  </Button>
                )}
                {order.status === 'Delivered' && (
                  <Button size="sm" variant="primary" onClick={handleComplete} loading={completeMut.isPending}>
                    <CheckCircle className="h-4 w-4" /> Complete Order
                  </Button>
                )}
                {!['Completed', 'Cancelled'].includes(order.status) && (
                  <Button size="sm" variant="danger" onClick={() => setShowCancelForm(!showCancelForm)}>
                    <XCircle className="h-4 w-4" /> Cancel
                  </Button>
                )}
              </div>
 
              {/* Cancel form */}
              {showCancelForm && (
                <form onSubmit={cancelForm.handleSubmit(handleCancel)} className="mt-4 space-y-3 rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] p-4">
                  <p className="text-sm font-medium text-[var(--text-primary)]">Cancellation reason</p>
                  <Textarea
                    rows={2}
                    placeholder="Customer request, stock issue..."
                    className="bg-[var(--bg-primary)]"
                    error={cancelForm.formState.errors.reason?.message}
                    {...cancelForm.register('reason')}
                  />
                  <div className="flex justify-end gap-2">
                    <Button variant="secondary" size="sm" type="button" onClick={() => { setShowCancelForm(false); cancelForm.reset(); }}>{t('common.cancel')}</Button>
                    <Button size="sm" variant="danger" type="submit" loading={cancelMut.isPending}>{t('common.confirm')}</Button>
                  </div>
                </form>
              )}
            </section>
 
            {/* Timestamps */}
            <div className="text-xs text-[var(--text-muted)] space-y-1">
              <p>Created: {formatDateTime(order.orderDate)}</p>
              {order.completedAt && <p>Completed: {formatDateTime(order.completedAt)}</p>}
              {order.cancelledAt && <p>Cancelled: {formatDateTime(order.cancelledAt)}</p>}
            </div>
        </div>
      ) : null}
    </SlidePanel>
  );
}