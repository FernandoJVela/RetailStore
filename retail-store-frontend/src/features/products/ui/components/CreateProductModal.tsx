import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslation } from 'react-i18next';
import { Modal, Button, Input } from '@shared/components/ui';
import { getApiErrorMessage } from '@shared/api/http-client';
import { useCreateProduct } from '@features/products/application/hooks/useProductsQueries';
import { PRODUCT_CATEGORIES } from '@features/products';
import { createProductSchema, type CreateProductFormData } from '@features/products';
import { useState } from 'react';
 
interface CreateProductModalProps {
  isOpen: boolean;
  onClose: () => void;
}
 
export function CreateProductModal({ isOpen, onClose }: CreateProductModalProps) {
  const { t } = useTranslation();
  const createMutation = useCreateProduct();
  const [apiError, setApiError] = useState('');
 
  const { register, handleSubmit, reset, formState: { errors } } = useForm<CreateProductFormData>({
    resolver: yupResolver(createProductSchema),
    defaultValues: { 
      name: '', 
      sku: '', 
      price: 0, 
      currency: 'USD', 
      category: '', 
      description: ''
    },
  });
 
  const onSubmit = async (data: CreateProductFormData) => {
    setApiError('');
    try {
      await createMutation.mutateAsync(data);
      reset();
      onClose();
    } catch (err) {
      setApiError(getApiErrorMessage(err));
    }
  };
 
  const handleClose = () => { reset(); setApiError(''); onClose(); };
 
  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Create Product" size="lg">
      {apiError && (
        <div className="mb-4 rounded-lg bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-800 p-3">
          <p className="text-sm text-red-700 dark:text-red-400">{apiError}</p>
        </div>
      )}
 
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Input label="Product Name" placeholder="Wireless Headphones" error={errors.name?.message} {...register('name')} />
          <Input label="SKU" placeholder="ELEC-WBH-001" error={errors.sku?.message} {...register('sku')} />
        </div>
 
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <Input label="Price" type="number" step="0.01" placeholder="79.99" error={errors.price?.message} {...register('price', { valueAsNumber: true })} />
          <Input label="Currency" placeholder="USD" error={errors.currency?.message} {...register('currency')} />
          <div className="space-y-1.5">
            <label className="block text-sm font-medium text-[var(--text-secondary)]">Category</label>
            <select
              className="w-full rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] px-3.5 py-2.5 text-sm text-[var(--text-primary)] focus:border-primary-500 focus:ring-1 focus:ring-primary-500 focus:outline-none"
              {...register('category')}
            >
              <option value="">Select category</option>
              {PRODUCT_CATEGORIES.map((cat) => (
                <option key={cat} value={cat}>{cat}</option>
              ))}
            </select>
            {errors.category && <p className="text-xs text-danger">{errors.category.message}</p>}
          </div>
        </div>
 
        <div className="space-y-1.5">
          <label className="block text-sm font-medium text-[var(--text-secondary)]">Description</label>
          <textarea
            rows={3}
            placeholder="Product description (optional)"
            className="w-full rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] px-3.5 py-2.5 text-sm text-[var(--text-primary)] placeholder:text-[var(--text-muted)] focus:border-primary-500 focus:ring-1 focus:ring-primary-500 focus:outline-none resize-none"
            {...register('description')}
          />
        </div>
 
        <div className="flex justify-end gap-3 pt-2">
          <Button variant="secondary" type="button" onClick={handleClose}>{t('common.cancel')}</Button>
          <Button type="submit" loading={createMutation.isPending}>{t('common.create')}</Button>
        </div>
      </form>
    </Modal>
  );
}