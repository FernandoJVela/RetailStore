import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslation } from 'react-i18next';
import { X, Pencil, DollarSign, Tag, FileText } from 'lucide-react';
import { Button, Input, Badge, Spinner } from '@shared/components/ui';
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
 
  if (!isOpen) return null;
 
  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm" onClick={onClose} />
      <div className="fixed inset-y-0 right-0 z-50 w-full max-w-lg overflow-y-auto bg-[var(--bg-secondary)] shadow-2xl">
        {/* Header */}
        <div className="sticky top-0 z-10 flex items-center justify-between border-b border-[var(--border-color)] bg-[var(--bg-secondary)] px-6 py-4">
          <h2 className="text-lg font-semibold text-[var(--text-primary)]">Product Details</h2>
          <button onClick={onClose} className="rounded-lg p-1.5 text-[var(--text-muted)] hover:bg-[var(--bg-tertiary)]">
            <X className="h-5 w-5" />
          </button>
        </div>
 
        {isLoading ? (
          <Spinner />
        ) : product ? (
          <div className="p-6 space-y-6">
            {apiError && (
              <div className="rounded-lg bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-800 p-3">
                <p className="text-sm text-red-700 dark:text-red-400">{apiError}</p>
              </div>
            )}
 
            {/* ─── Product Info Section ───────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider flex items-center gap-2">
                  <Tag className="h-4 w-4" /> Details
                </h3>
                <button onClick={() => setEditMode(editMode === 'details' ? 'none' : 'details')} className="text-primary-600 hover:text-primary-500">
                  <Pencil className="h-4 w-4" />
                </button>
              </div>
 
              {editMode === 'details' ? (
                <form onSubmit={detailsForm.handleSubmit(handleDetailsSave)} className="space-y-3">
                  <Input label="Name" error={detailsForm.formState.errors.name?.message} {...detailsForm.register('name')} />
                  <div className="space-y-1.5">
                    <label className="block text-sm font-medium text-[var(--text-secondary)]">Category</label>
                    <select
                      className="w-full rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] px-3.5 py-2.5 text-sm text-[var(--text-primary)] focus:border-primary-500 focus:outline-none"
                      {...detailsForm.register('category')}
                    >
                      {PRODUCT_CATEGORIES.map((cat) => <option key={cat} value={cat}>{cat}</option>)}
                    </select>
                  </div>
                  <div className="space-y-1.5">
                    <label className="block text-sm font-medium text-[var(--text-secondary)]">Description</label>
                    <textarea
                      rows={3}
                      className="w-full rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] px-3.5 py-2.5 text-sm text-[var(--text-primary)] focus:border-primary-500 focus:outline-none resize-none"
                      {...detailsForm.register('description')}
                    />
                  </div>
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
            </section>
 
            {/* ─── Price Section ──────────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider flex items-center gap-2">
                  <DollarSign className="h-4 w-4" /> Pricing
                </h3>
                <button onClick={() => setEditMode(editMode === 'price' ? 'none' : 'price')} className="text-primary-600 hover:text-primary-500">
                  <Pencil className="h-4 w-4" />
                </button>
              </div>
 
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
            </section>
          </div>
        ) : null}
      </div>
    </>
  );
}