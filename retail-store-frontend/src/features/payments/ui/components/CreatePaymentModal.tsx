import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Modal, Button, Select, Input, Alert } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import { useCreatePayment } from '@features/payments/application/hooks/usePaymentsQueries';
import { PAYMENT_METHODS, PAYMENT_METHOD_LABELS } from '@features/payments';
import { useOrders } from '@features/orders';
import { formatDate } from '@shared/lib/utils';

interface CreatePaymentModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CreatePaymentModal({ isOpen, onClose }: CreatePaymentModalProps) {
  const { t } = useTranslation();
  const createMutation = useCreatePayment();

  const { data: confirmedOrders } = useOrders({ status: 'Confirmed' });

  const [orderId, setOrderId] = useState('');
  const [method, setMethod] = useState('');
  const [methodDetail, setMethodDetail] = useState('');
  const [gatewayName, setGatewayName] = useState('');
  const [apiError, setApiError] = useState('');

  const selectedOrder = confirmedOrders?.find((o) => o.id === orderId);

  const handleSubmit = async () => {
    if (!orderId || !method) return;
    setApiError('');
    try {
      await createMutation.mutateAsync({
        orderId,
        method,
        methodDetail: methodDetail.trim() || undefined,
        gatewayName: gatewayName.trim() || undefined,
      });
      handleClose();
    } catch (err) {
      setApiError(getApiErrorMessage(err));
    }
  };

  const handleClose = () => {
    setOrderId('');
    setMethod('');
    setMethodDetail('');
    setGatewayName('');
    setApiError('');
    onClose();
  };

  const methodDetailPlaceholder: Record<string, string> = {
    CreditCard: 'e.g. **** **** **** 4532',
    DebitCard: 'e.g. **** **** **** 7891',
    BankTransfer: 'e.g. Bancolombia #4456',
    DigitalWallet: 'e.g. Nequi',
    PSE: 'e.g. Davivienda',
    Cash: '',
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title={t('payments.createPayment')}>
      {apiError && <Alert message={apiError} className="mb-4" />}

      <div className="space-y-5">
        {/* Order selector */}
        <Select
          label={t('payments.selectOrder')}
          value={orderId}
          onChange={(e) => setOrderId(e.target.value)}
        >
          <option value="">{t('payments.chooseConfirmedOrder')}</option>
          {confirmedOrders?.map((o) => (
            <option key={o.id} value={o.id}>
              #{o.id.substring(0, 8).toUpperCase()} — {o.formattedTotal} — {formatDate(o.orderDate)}
            </option>
          ))}
        </Select>

        {/* Show selected order total */}
        {selectedOrder && (
          <div className="rounded-lg bg-[var(--bg-secondary)] border border-[var(--border-color)] px-4 py-3 text-sm">
            <span className="text-[var(--text-secondary)]">{t('payments.orderTotal')}: </span>
            <span className="font-semibold text-[var(--text-primary)]">{selectedOrder.formattedTotal}</span>
            <span className="ml-3 text-[var(--text-muted)]">({selectedOrder.itemCount} {selectedOrder.itemCount === 1 ? 'item' : 'items'})</span>
          </div>
        )}

        {/* Payment method */}
        <Select
          label={t('payments.method')}
          value={method}
          onChange={(e) => setMethod(e.target.value)}
        >
          <option value="">{t('payments.chooseMethod')}</option>
          {PAYMENT_METHODS.map((m) => (
            <option key={m} value={m}>{PAYMENT_METHOD_LABELS[m]}</option>
          ))}
        </Select>

        {/* Method detail (optional, hidden for Cash) */}
        {method && method !== 'Cash' && (
          <Input
            label={`${t('payments.methodDetail')} (${t('common.optional')})`}
            value={methodDetail}
            onChange={(e) => setMethodDetail(e.target.value)}
            placeholder={methodDetailPlaceholder[method] ?? ''}
          />
        )}

        {/* Gateway name (optional) */}
        <Input
          label={`${t('payments.gateway')} (${t('common.optional')})`}
          value={gatewayName}
          onChange={(e) => setGatewayName(e.target.value)}
          placeholder="e.g. Stripe, PayU, Mercadopago"
        />

        {/* Actions */}
        <div className="flex justify-end gap-3 pt-2">
          <Button variant="secondary" onClick={handleClose}>{t('common.cancel')}</Button>
          <Button
            onClick={handleSubmit}
            loading={createMutation.isPending}
            disabled={!orderId || !method}
          >
            {t('payments.createPayment')}
          </Button>
        </div>
      </div>
    </Modal>
  );
}
