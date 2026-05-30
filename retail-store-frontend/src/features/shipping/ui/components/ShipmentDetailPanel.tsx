import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslation } from 'react-i18next';
import { X, Truck, MapPin, Package, CheckCircle, XCircle } from 'lucide-react';
import { Button, Input, Badge, Spinner } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import { formatDate, formatDateTime } from '@shared/lib/utils';
import { cn } from '@shared/lib/utils';
import {
  useShipment, useAssignCarrier, useMarkShipped,
  useMarkInTransit, useMarkDelivered, useMarkFailed, useCancelShipment,
} from '@features/shipping/application/hooks/useShippingQueries';
import { shipmentStatusVariant, STATUS_STEPS } from '@features/shipping';
import type { ShipmentStatus } from '@features/shipping';
import {
  assignCarrierSchema, reasonSchema,
  type AssignCarrierFormData, type ReasonFormData,
} from '@features/shipping/application/useCases/shipping.validation';
 
interface ShipmentDetailPanelProps {
  shipmentId: string;
  isOpen: boolean;
  onClose: () => void;
}
 
export function ShipmentDetailPanel({ shipmentId, isOpen, onClose }: ShipmentDetailPanelProps) {
  const { t } = useTranslation();
  const { data: shipment, isLoading } = useShipment(shipmentId);
  const [showCarrierForm, setShowCarrierForm] = useState(false);
  const [showReasonForm, setShowReasonForm] = useState<'fail' | 'cancel' | null>(null);
  const [apiError, setApiError] = useState('');
 
  const assignCarrierMut = useAssignCarrier();
  const shipMut = useMarkShipped();
  const transitMut = useMarkInTransit();
  const deliverMut = useMarkDelivered();
  const failMut = useMarkFailed();
  const cancelMut = useCancelShipment();
 
  const carrierForm = useForm<AssignCarrierFormData>({
    resolver: yupResolver(assignCarrierSchema),
    defaultValues: { carrier: '', trackingNumber: '', estimatedDelivery: '' },
  });
 
  const reasonForm = useForm<ReasonFormData>({
    resolver: yupResolver(reasonSchema),
    defaultValues: { reason: '' },
  });
 
  const handleCarrierAssign = async (data: AssignCarrierFormData) => {
    setApiError('');
    try {
      await assignCarrierMut.mutateAsync({ id: shipmentId, data });
      setShowCarrierForm(false);
      carrierForm.reset();
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  const handleLifecycle = async (action: 'ship' | 'transit' | 'deliver') => {
    setApiError('');
    try {
      if (action === 'ship') await shipMut.mutateAsync(shipmentId);
      if (action === 'transit') await transitMut.mutateAsync(shipmentId);
      if (action === 'deliver') await deliverMut.mutateAsync(shipmentId);
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  const handleReasonAction = async (data: ReasonFormData) => {
    setApiError('');
    try {
      if (showReasonForm === 'fail') await failMut.mutateAsync({ id: shipmentId, reason: data.reason });
      if (showReasonForm === 'cancel') await cancelMut.mutateAsync({ id: shipmentId, reason: data.reason });
      setShowReasonForm(null);
      reasonForm.reset();
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  if (!isOpen) return null;
 
  // Progress tracker index
  const currentStepIdx = shipment ? STATUS_STEPS.indexOf(shipment.status as ShipmentStatus) : -1;
  const isFinalBad = shipment && ['Failed', 'Cancelled', 'Returned'].includes(shipment.status);
 
  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm" onClick={onClose} />
      <div className="fixed inset-y-0 right-0 z-50 w-full max-w-lg overflow-y-auto bg-[var(--bg-secondary)] shadow-2xl">
        <div className="sticky top-0 z-10 flex items-center justify-between border-b border-[var(--border-color)] bg-[var(--bg-secondary)] px-6 py-4">
          <h2 className="text-lg font-semibold text-[var(--text-primary)]">Shipment Details</h2>
          <button onClick={onClose} className="rounded-lg p-1.5 text-[var(--text-muted)] hover:bg-[var(--bg-tertiary)]">
            <X className="h-5 w-5" />
          </button>
        </div>
 
        {isLoading ? (
          <Spinner />
        ) : shipment ? (
          <div className="p-6 space-y-6">
            {apiError && (
              <div className="rounded-lg bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-800 p-3">
                <p className="text-sm text-red-700 dark:text-red-400">{apiError}</p>
              </div>
            )}
 
            {/* ─── Progress Tracker ──────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <div className="flex items-center justify-between mb-4">
                <Badge variant={shipmentStatusVariant(shipment.status)}>
                  {shipment.status === 'InTransit' ? 'In Transit' : shipment.status}
                </Badge>
                <span className="text-xs text-[var(--text-muted)]">{shipment.formattedCost}</span>
              </div>
 
              {/* Step indicator */}
              {!isFinalBad && (
                <div className="flex items-center gap-1 mb-2">
                  {STATUS_STEPS.map((step, idx) => (
                    <div key={step} className="flex-1 flex items-center">
                      <div className={cn(
                        'h-2 w-full rounded-full transition-colors',
                        idx <= currentStepIdx ? 'bg-primary-600' : 'bg-[var(--bg-tertiary)]'
                      )} />
                    </div>
                  ))}
                </div>
              )}
              <div className="flex justify-between text-xs text-[var(--text-muted)]">
                {!isFinalBad ? STATUS_STEPS.map((step) => (
                  <span key={step}>{step === 'InTransit' ? 'Transit' : step}</span>
                )) : (
                  <span className="text-red-500 font-medium">{shipment.status}</span>
                )}
              </div>
            </section>
 
            {/* ─── Carrier & Tracking ────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3 flex items-center gap-2">
                <Truck className="h-4 w-4" /> Carrier & Tracking
              </h3>
 
              {shipment.hasCarrier ? (
                <div className="space-y-2">
                  <p className="text-sm text-[var(--text-primary)]"><strong>{shipment.carrier}</strong></p>
                  <p className="text-xs font-mono text-[var(--text-secondary)]">{shipment.trackingNumber}</p>
                  {shipment.estimatedDelivery && (
                    <p className="text-xs text-[var(--text-muted)]">Est. delivery: {formatDate(shipment.estimatedDelivery)}</p>
                  )}
                </div>
              ) : showCarrierForm ? (
                <form onSubmit={carrierForm.handleSubmit(handleCarrierAssign)} className="space-y-3">
                  <Input label="Carrier" placeholder="Servientrega, InterRapidisimo..." error={carrierForm.formState.errors.carrier?.message} {...carrierForm.register('carrier')} />
                  <Input label="Tracking Number" placeholder="SRV-123456789" error={carrierForm.formState.errors.trackingNumber?.message} {...carrierForm.register('trackingNumber')} />
                  <Input label="Est. Delivery (optional)" type="date" {...carrierForm.register('estimatedDelivery')} />
                  <div className="flex justify-end gap-2">
                    <Button variant="secondary" size="sm" type="button" onClick={() => setShowCarrierForm(false)}>{t('common.cancel')}</Button>
                    <Button size="sm" type="submit" loading={assignCarrierMut.isPending}>{t('common.save')}</Button>
                  </div>
                </form>
              ) : (
                <div>
                  <p className="text-sm text-[var(--text-muted)] italic mb-2">No carrier assigned</p>
                  <Button size="sm" variant="outline" onClick={() => setShowCarrierForm(true)}>Assign Carrier</Button>
                </div>
              )}
            </section>
 
            {/* ─── Address ───────────────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3 flex items-center gap-2">
                <MapPin className="h-4 w-4" /> Delivery Address
              </h3>
              <p className="text-sm text-[var(--text-secondary)]">{shipment.address.fullAddress}</p>
            </section>
 
            {/* ─── Items ─────────────────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3 flex items-center gap-2">
                <Package className="h-4 w-4" /> Items ({shipment.items.length})
              </h3>
              <div className="space-y-2">
                {shipment.items.map((item) => (
                  <div key={item.id} className="flex items-center justify-between rounded-lg bg-[var(--bg-secondary)] px-3 py-2 border border-[var(--border-color)]">
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-[var(--text-primary)] truncate">{item.productName}</p>
                    </div>
                    <div className="text-right shrink-0 ml-4">
                      <span className="text-sm font-semibold text-[var(--text-primary)]">×{item.quantity}</span>
                      {item.weightKg && <p className="text-xs text-[var(--text-muted)]">{item.weightKg}kg</p>}
                    </div>
                  </div>
                ))}
                {shipment.totalWeightKg && (
                  <p className="text-xs text-[var(--text-muted)] text-right pt-1">Total weight: {shipment.totalWeightKg}kg</p>
                )}
              </div>
            </section>
 
            {/* ─── Actions ───────────────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3">Actions</h3>
              <div className="flex flex-wrap gap-2">
                {shipment.status === 'Processing' && shipment.hasCarrier && (
                  <Button size="sm" onClick={() => handleLifecycle('ship')} loading={shipMut.isPending}>
                    <Truck className="h-4 w-4" /> Mark Shipped
                  </Button>
                )}
                {shipment.status === 'Shipped' && (
                  <Button size="sm" onClick={() => handleLifecycle('transit')} loading={transitMut.isPending}>
                    <Truck className="h-4 w-4" /> Mark In Transit
                  </Button>
                )}
                {shipment.status === 'InTransit' && (
                  <Button size="sm" variant="primary" onClick={() => handleLifecycle('deliver')} loading={deliverMut.isPending}>
                    <CheckCircle className="h-4 w-4" /> Mark Delivered
                  </Button>
                )}
                {!['Delivered', 'Failed', 'Cancelled', 'Returned'].includes(shipment.status) && (
                  <>
                    <Button size="sm" variant="outline" onClick={() => setShowReasonForm('fail')}>
                      <XCircle className="h-4 w-4" /> Mark Failed
                    </Button>
                    <Button size="sm" variant="danger" onClick={() => setShowReasonForm('cancel')}>
                      Cancel
                    </Button>
                  </>
                )}
              </div>
 
              {showReasonForm && (
                <form onSubmit={reasonForm.handleSubmit(handleReasonAction)} className="mt-4 space-y-3 rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] p-4">
                  <p className="text-sm font-medium text-[var(--text-primary)]">
                    {showReasonForm === 'fail' ? 'Reason for failure' : 'Reason for cancellation'}
                  </p>
                  <textarea
                    rows={2}
                    placeholder="Describe the reason..."
                    className="w-full rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] px-3.5 py-2.5 text-sm text-[var(--text-primary)] placeholder:text-[var(--text-muted)] focus:border-primary-500 focus:outline-none resize-none"
                    {...reasonForm.register('reason')}
                  />
                  {reasonForm.formState.errors.reason && (
                    <p className="text-xs text-danger">{reasonForm.formState.errors.reason.message}</p>
                  )}
                  <div className="flex justify-end gap-2">
                    <Button variant="secondary" size="sm" type="button" onClick={() => { setShowReasonForm(null); reasonForm.reset(); }}>{t('common.cancel')}</Button>
                    <Button size="sm" variant="danger" type="submit" loading={failMut.isPending || cancelMut.isPending}>{t('common.confirm')}</Button>
                  </div>
                </form>
              )}
            </section>
 
            {/* Timestamps */}
            <div className="text-xs text-[var(--text-muted)] space-y-1">
              <p>Created: {formatDateTime(shipment.createdAt)}</p>
              {shipment.shippedAt && <p>Shipped: {formatDateTime(shipment.shippedAt)}</p>}
              {shipment.deliveredAt && <p>Delivered: {formatDateTime(shipment.deliveredAt)}</p>}
            </div>
          </div>
        ) : null}
      </div>
    </>
  );
}