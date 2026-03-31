import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslation } from 'react-i18next';
import { Plus, Minus, ArrowLeftRight, Settings2 } from 'lucide-react';
import { Modal, Button, Input } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import {
  useAddStock, useRemoveStock, useAdjustStock, useUpdateThreshold,
} from '@features/inventory/application/hooks/useInventoryQueries';
import {
  addStockSchema, adjustStockSchema, updateThresholdSchema,
  type AddStockFormData, type AdjustStockFormData, type UpdateThresholdFormData,
} from '@features/inventory/application/useCases/inventory.validation';
import type { InventoryItem } from '@features/inventory';
import { useState } from 'react';
 
type ActionType = 'add' | 'remove' | 'adjust' | 'threshold';
 
interface StockActionModalProps {
  item: InventoryItem;
  action: ActionType;
  isOpen: boolean;
  onClose: () => void;
}
 
const config: Record<ActionType, { title: string; icon: typeof Plus; color: string }> = {
  add: { title: 'Add Stock', icon: Plus, color: 'text-emerald-600' },
  remove: { title: 'Remove Stock', icon: Minus, color: 'text-red-600' },
  adjust: { title: 'Adjust Quantity', icon: ArrowLeftRight, color: 'text-primary-600' },
  threshold: { title: 'Update Reorder Threshold', icon: Settings2, color: 'text-[var(--text-secondary)]' },
};
 
export function StockActionModal({ item, action, isOpen, onClose }: StockActionModalProps) {
  const { t } = useTranslation();
  const [apiError, setApiError] = useState('');
  const addMutation = useAddStock();
  const removeMutation = useRemoveStock();
  const adjustMutation = useAdjustStock();
  const thresholdMutation = useUpdateThreshold();
 
  const { title, icon: Icon, color } = config[action];
 
  // ─── Add / Remove form ────────────────────────────────────
  const qtyForm = useForm<AddStockFormData>({
    resolver: yupResolver(addStockSchema),
    defaultValues: { quantity: 1 },
  });
 
  // ─── Adjust form ──────────────────────────────────────────
  const adjustForm = useForm<AdjustStockFormData>({
    resolver: yupResolver(adjustStockSchema),
    defaultValues: { newQuantity: item.quantityOnHand, reason: '' },
  });
 
  // ─── Threshold form ───────────────────────────────────────
  const thresholdForm = useForm<UpdateThresholdFormData>({
    resolver: yupResolver(updateThresholdSchema),
    defaultValues: { newThreshold: item.reorderThreshold },
  });
 
  const isPending = addMutation.isPending || removeMutation.isPending || adjustMutation.isPending || thresholdMutation.isPending;
 
  const handleSubmit = async () => {
    setApiError('');
    try {
      if (action === 'add') {
        await qtyForm.handleSubmit(async (data) => {
          await addMutation.mutateAsync({ productId: item.productId, quantity: data.quantity });
          onClose();
        })();
      } else if (action === 'remove') {
        await qtyForm.handleSubmit(async (data) => {
          await removeMutation.mutateAsync({ productId: item.productId, quantity: data.quantity });
          onClose();
        })();
      } else if (action === 'adjust') {
        await adjustForm.handleSubmit(async (data) => {
          await adjustMutation.mutateAsync({ productId: item.productId, data });
          onClose();
        })();
      } else if (action === 'threshold') {
        await thresholdForm.handleSubmit(async (data) => {
          await thresholdMutation.mutateAsync({ productId: item.productId, newThreshold: data.newThreshold });
          onClose();
        })();
      }
    } catch (err) {
      setApiError(getApiErrorMessage(err));
    }
  };
 
  return (
    <Modal isOpen={isOpen} onClose={onClose} title={title} size="sm">
      {/* Item context */}
      <div className="mb-4 rounded-lg bg-[var(--bg-primary)] border border-[var(--border-color)] p-3">
        <div className="flex items-center gap-3">
          <div className={`flex h-9 w-9 items-center justify-center rounded-lg bg-[var(--bg-tertiary)]`}>
            <Icon className={`h-5 w-5 ${color}`} />
          </div>
          <div className="min-w-0">
            <p className="font-medium text-[var(--text-primary)] truncate">{item.productName}</p>
            <p className="text-xs text-[var(--text-muted)]">
              On hand: {item.quantityOnHand} · Reserved: {item.reservedQuantity} · Available: {item.availableQuantity}
            </p>
          </div>
        </div>
      </div>
 
      {apiError && (
        <div className="mb-4 rounded-lg bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-800 p-3">
          <p className="text-sm text-red-700 dark:text-red-400">{apiError}</p>
        </div>
      )}
 
      {/* Form content based on action */}
      <div className="space-y-4">
        {(action === 'add' || action === 'remove') && (
          <Input
            label={action === 'add' ? 'Quantity to add' : 'Quantity to remove'}
            type="number"
            min={1}
            error={qtyForm.formState.errors.quantity?.message}
            {...qtyForm.register('quantity', { valueAsNumber: true })}
          />
        )}
 
        {action === 'adjust' && (
          <>
            <Input
              label="New quantity on hand"
              type="number"
              min={0}
              error={adjustForm.formState.errors.newQuantity?.message}
              {...adjustForm.register('newQuantity', { valueAsNumber: true })}
            />
            <div className="space-y-1.5">
              <label className="block text-sm font-medium text-[var(--text-secondary)]">Reason for adjustment</label>
              <textarea
                rows={2}
                placeholder="e.g., Physical count correction, damage write-off..."
                className="w-full rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] px-3.5 py-2.5 text-sm text-[var(--text-primary)] placeholder:text-[var(--text-muted)] focus:border-primary-500 focus:outline-none resize-none"
                {...adjustForm.register('reason')}
              />
              {adjustForm.formState.errors.reason && (
                <p className="text-xs text-danger">{adjustForm.formState.errors.reason.message}</p>
              )}
            </div>
          </>
        )}
 
        {action === 'threshold' && (
          <Input
            label="New reorder threshold"
            type="number"
            min={0}
            error={thresholdForm.formState.errors.newThreshold?.message}
            {...thresholdForm.register('newThreshold', { valueAsNumber: true })}
          />
        )}
 
        <div className="flex justify-end gap-3 pt-2">
          <Button variant="secondary" onClick={onClose}>{t('common.cancel')}</Button>
          <Button onClick={handleSubmit} loading={isPending}>
            {t('common.confirm')}
          </Button>
        </div>
      </div>
    </Modal>
  );
}