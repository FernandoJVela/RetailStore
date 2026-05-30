import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslation } from 'react-i18next';
import { Modal, Button, Input, Textarea, Select, Alert } from '@shared/components/ui';
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
      {apiError && <Alert message={apiError} className="mb-4" />}
 
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Input label="Product Name" placeholder="Wireless Headphones" error={errors.name?.message} {...register('name')} />
          <Input label="SKU" placeholder="ELEC-WBH-001" error={errors.sku?.message} {...register('sku')} />
        </div>
 
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <Input label="Price" type="number" step="0.01" placeholder="79.99" error={errors.price?.message} {...register('price', { valueAsNumber: true })} />
          <Input label="Currency" placeholder="USD" error={errors.currency?.message} {...register('currency')} />
          <Select
            label="Category"
            error={errors.category?.message}
            {...register('category')}
          >
            <option value="">Select category</option>
            {PRODUCT_CATEGORIES.map((cat) => (
              <option key={cat} value={cat}>{cat}</option>
            ))}
          </Select>
        </div>
 
        <Textarea
          label="Description"
          rows={3}
          placeholder="Product description (optional)"
          {...register('description')}
        />
 
        <div className="flex justify-end gap-3 pt-2">
          <Button variant="secondary" type="button" onClick={handleClose}>{t('common.cancel')}</Button>
          <Button type="submit" loading={createMutation.isPending}>{t('common.create')}</Button>
        </div>
      </form>
    </Modal>
  );
}