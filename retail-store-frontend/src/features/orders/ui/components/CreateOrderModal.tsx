import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Trash2 } from 'lucide-react';
import { Modal, Button, Input, Select, Alert } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import { useCreateOrder } from '@features/orders/application/hooks/useOrdersQueries';
import { useCustomers } from '@features/customers/application/hooks/useCustomersQueries';
import { useProducts } from '@features/products/application/hooks/useProductsQueries';
 
interface CreateOrderModalProps {
  isOpen: boolean;
  onClose: () => void;
}
 
interface LineItem {
  productId: string;
  productName: string;
  quantity: number;
}
 
export function CreateOrderModal({ isOpen, onClose }: CreateOrderModalProps) {
  const { t } = useTranslation();
  const createMutation = useCreateOrder();
  const { data: customers } = useCustomers({ isActive: true });
  const { data: products } = useProducts({ isActive: true });
  const [customerId, setCustomerId] = useState('');
  const [lineItems, setLineItems] = useState<LineItem[]>([]);
  const [selectedProduct, setSelectedProduct] = useState('');
  const [quantity, setQuantity] = useState(1);
  const [apiError, setApiError] = useState('');
 
  const addItem = () => {
    if (!selectedProduct || quantity < 1) return;
    const product = products?.find((p) => p.id === selectedProduct);
    if (!product) return;
    if (lineItems.some((li) => li.productId === selectedProduct)) return;
    setLineItems([...lineItems, { productId: product.id, productName: product.name, quantity }]);
    setSelectedProduct('');
    setQuantity(1);
  };
 
  const removeItem = (productId: string) => {
    setLineItems(lineItems.filter((li) => li.productId !== productId));
  };
 
  const handleSubmit = async () => {
    if (!customerId || lineItems.length === 0) return;
    setApiError('');
    try {
      await createMutation.mutateAsync({
        customerId,
        items: lineItems.map((li) => ({ productId: li.productId, quantity: li.quantity })),
      });
      handleClose();
    } catch (err) {
      setApiError(getApiErrorMessage(err));
    }
  };
 
  const handleClose = () => {
    setCustomerId('');
    setLineItems([]);
    setSelectedProduct('');
    setQuantity(1);
    setApiError('');
    onClose();
  };
 
  const availableProducts = products?.filter((p) => !lineItems.some((li) => li.productId === p.id)) ?? [];
 
  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Create Order" size="lg">
      {apiError && <Alert message={apiError} className="mb-4" />}
 
      <div className="space-y-5">
        {/* Customer select */}
        <Select
          label="Customer"
          value={customerId}
          onChange={(e) => setCustomerId(e.target.value)}
        >
          <option value="">Select a customer</option>
          {customers?.map((c) => (
            <option key={c.id} value={c.id}>{c.fullName} ({c.email})</option>
          ))}
        </Select>
 
        {/* Add item row */}
        <div className="space-y-1.5">
          <label className="block text-sm font-medium text-[var(--text-secondary)]">Add items</label>
          <div className="flex gap-2">
            <Select
              value={selectedProduct}
              onChange={(e) => setSelectedProduct(e.target.value)}
              className="flex-1"
            >
              <option value="">Select product</option>
              {availableProducts.map((p) => (
                <option key={p.id} value={p.id}>{p.name} ({p.formattedPrice})</option>
              ))}
            </Select>
            <Input
              type="number"
              min={1}
              value={quantity}
              onChange={(e) => setQuantity(Number(e.target.value))}
              className="w-20 text-center tabular-nums"
            />
            <Button size="md" variant="outline" onClick={addItem} disabled={!selectedProduct}>
              <Plus className="h-4 w-4" />
            </Button>
          </div>
        </div>
 
        {/* Line items list */}
        {lineItems.length > 0 && (
          <div className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] divide-y divide-[var(--border-color)]">
            {lineItems.map((li) => (
              <div key={li.productId} className="flex items-center justify-between px-4 py-3">
                <div className="min-w-0">
                  <p className="text-sm font-medium text-[var(--text-primary)] truncate">{li.productName}</p>
                </div>
                <div className="flex items-center gap-3 shrink-0 ml-4">
                  <span className="text-sm text-[var(--text-secondary)] tabular-nums">×{li.quantity}</span>
                  <button
                    onClick={() => removeItem(li.productId)}
                    className="rounded p-1 text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10"
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
 
        {lineItems.length === 0 && (
          <p className="text-sm text-[var(--text-muted)] italic text-center py-4">
            No items added yet. Select a product and click + to add.
          </p>
        )}
 
        {/* Actions */}
        <div className="flex justify-end gap-3 pt-2">
          <Button variant="secondary" onClick={handleClose}>{t('common.cancel')}</Button>
          <Button
            onClick={handleSubmit}
            loading={createMutation.isPending}
            disabled={!customerId || lineItems.length === 0}
          >
            Create Order ({lineItems.length} {lineItems.length === 1 ? 'item' : 'items'})
          </Button>
        </div>
      </div>
    </Modal>
  );
}