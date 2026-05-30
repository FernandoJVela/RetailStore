import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslation } from 'react-i18next';
import { DollarSign, Tag, FileText } from 'lucide-react';
import { Button, Input, Textarea, Select, Badge, Spinner, SlidePanel, Alert, DetailSection } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import { formatDate } from '@shared/lib/utils';
import { useProduct, useUpdateProductDetails, useUpdateProductPrice } from '@features/products/application/hooks/useProductsQueries';
import { PRODUCT_CATEGORIES } from '@features/products';
import {
  updateDetailsSchema, updatePriceSchema,
  type UpdateDetailsFormData, type UpdatePriceFormData,
} from '@features/products/application/useCases/products.validation';
 
interface ProductDetailPanelProps {
  productId: string;
  isOpen: boolean;
  onClose: () => void;
}
 
type EditMode = 'none' | 'details' | 'price';
 
export function ProductDetailPanel({ productId, isOpen, onClose }: ProductDetailPanelProps) {
  const { t } = useTranslation();
  const { data: product, isLoading } = useProduct(productId);
  const [editMode, setEditMode] = useState<EditMode>('none');
  const [apiError, setApiError] = useState('');
 
  const detailsMutation = useUpdateProductDetails();
  const priceMutation = useUpdateProductPrice();
 
  // Details form
  const detailsForm = useForm<UpdateDetailsFormData>({
    resolver: yupResolver(updateDetailsSchema),
    values: product ? { name: product.name, category: product.category, description: product.description || '' } : undefined,
  });
 
  const handleDetailsSave = async (data: UpdateDetailsFormData) => {
    setApiError('');
    try {
      await detailsMutation.mutateAsync({ id: productId, data });
      setEditMode('none');
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  // Price form
  const priceForm = useForm<UpdatePriceFormData>({
    resolver: yupResolver(updatePriceSchema),
    values: product ? { price: product.price, currency: product.currency } : undefined,
  });
 
  const handlePriceSave = async (data: UpdatePriceFormData) => {
    setApiError('');
    try {
      await priceMutation.mutateAsync({ id: productId, data });
      setEditMode('none');
    } catch (err) { setApiError(getApiErrorMessage(err)); }
  };
 
  return (
    <SlidePanel isOpen={isOpen} onClose={onClose} title="Product Details">
      {isLoading ? (
        <Spinner />
      ) : product ? (
        <div className="p-6 space-y-6">
            {apiError && <Alert message={apiError} />}
 
            {/* ─── Product Info Section ───────────────────── */}
            <DetailSection
              title="Details"
              icon={Tag}
              onEdit={() => setEditMode(editMode === 'details' ? 'none' : 'details')}
            >
 
              {editMode === 'details' ? (
                <form onSubmit={detailsForm.handleSubmit(handleDetailsSave)} className="space-y-3">
                  <Input label="Name" error={detailsForm.formState.errors.name?.message} {...detailsForm.register('name')} />
                  <Select
                    label="Category"
                    {...detailsForm.register('category')}
                  >
                    {PRODUCT_CATEGORIES.map((cat) => <option key={cat} value={cat}>{cat}</option>)}
                  </Select>
                  <Textarea
                    label="Description"
                    rows={3}
                    {...detailsForm.register('description')}
                  />
                  <div className="flex justify-end gap-2 pt-1">
                    <Button variant="secondary" size="sm" type="button" onClick={() => setEditMode('none')}>{t('common.cancel')}</Button>
                    <Button size="sm" type="submit" loading={detailsMutation.isPending}>{t('common.save')}</Button>
                  </div>
                </form>
              ) : (
                <div className="space-y-3">
                  <div>
                    <p className="text-lg font-semibold text-[var(--text-primary)]">{product.name}</p>
                    <p className="text-sm text-[var(--text-muted)] font-mono">{product.sku}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <Badge variant="default">{product.category}</Badge>
                    <Badge variant={product.isActive ? 'success' : 'danger'}>{product.statusLabel}</Badge>
                  </div>
                  {product.description && (
                    <div className="flex items-start gap-2 text-sm text-[var(--text-secondary)]">
                      <FileText className="h-4 w-4 mt-0.5 shrink-0" />
                      <span>{product.description}</span>
                    </div>
                  )}
                  <p className="text-xs text-[var(--text-muted)]">
                    Created {formatDate(product.createdAt)}
                    {product.updatedAt && ` · Updated ${formatDate(product.updatedAt)}`}
                  </p>
                </div>
              )}
            </DetailSection>

            {/* ─── Price Section ──────────────────────────── */}
            <DetailSection
              title="Pricing"
              icon={DollarSign}
              onEdit={() => setEditMode(editMode === 'price' ? 'none' : 'price')}
            >
 
              {editMode === 'price' ? (
                <form onSubmit={priceForm.handleSubmit(handlePriceSave)} className="space-y-3">
                  <div className="grid grid-cols-2 gap-3">
                    <Input label="Price" type="number" step="0.01" error={priceForm.formState.errors.price?.message} {...priceForm.register('price', { valueAsNumber: true })} />
                    <Input label="Currency" error={priceForm.formState.errors.currency?.message} {...priceForm.register('currency')} />
                  </div>
                  <div className="flex justify-end gap-2 pt-1">
                    <Button variant="secondary" size="sm" type="button" onClick={() => setEditMode('none')}>{t('common.cancel')}</Button>
                    <Button size="sm" type="submit" loading={priceMutation.isPending}>{t('common.save')}</Button>
                  </div>
                </form>
              ) : (
                <p className="text-3xl font-bold text-[var(--text-primary)]">{product.formattedPrice}</p>
              )}
            </DetailSection>
        </div>
      ) : null}
    </SlidePanel>
  );
}