import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Package, LayoutGrid, List } from 'lucide-react';
import { Button, Card, Spinner, EmptyState, SearchInput, FilterPillBar, PageHeader } from '@shared/components/ui';
import { useDebounce } from '@shared/hooks';
import { useProducts } from '@features/products/application/hooks/useProductsQueries';
import { PRODUCT_CATEGORIES } from '@features/products';
import { ProductCard } from '@features/products/ui/components/ProductCard';
import { ProductRow } from '@features/products/ui/components/ProductRow';
import { CreateProductModal } from '@features/products/ui/components/CreateProductModal';
import { ProductDetailPanel } from '@features/products/ui/components/ProductDetailPanel';
 
type ViewMode = 'grid' | 'table';
 
export function ProductsListPage() {
  const { t } = useTranslation();
  const [search, setSearch] = useState('');
  const [categoryFilter, setCategoryFilter] = useState<string>('all');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'inactive'>('all');
  const [viewMode, setViewMode] = useState<ViewMode>('grid');
  const [showCreate, setShowCreate] = useState(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);
 
  const debouncedSearch = useDebounce(search, 300);
  const { data: products, isLoading } = useProducts({
    search: debouncedSearch || undefined,
    category: categoryFilter === 'all' ? undefined : categoryFilter,
    isActive: statusFilter === 'all' ? undefined : statusFilter === 'active',
  });
 
  return (
    <div className="space-y-6">
      {/* Header */}
      <PageHeader
        title={t('nav.products')}
        subtitle={t('products.subtitle', { count: products?.length ?? 0 })}
        action={
          <Button onClick={() => setShowCreate(true)}>
            <Plus className="h-4 w-4" />
            <span className="hidden sm:inline">{t('products.createProduct')}</span>
            <span className="sm:hidden">{t('common.new')}</span>
          </Button>
        }
      />
 
      {/* Filters bar */}
      <Card>
        <div className="flex flex-col gap-4 lg:flex-row lg:items-center">
          {/* Search */}
          <SearchInput
            value={search}
            onChange={setSearch}
            placeholder={t('products.searchPlaceholder')}
          />
 
          {/* Category filter */}
          <FilterPillBar
            options={[
              { key: 'all', label: t('common.all') },
              ...PRODUCT_CATEGORIES.map((cat) => ({ key: cat, label: cat })),
            ]}
            value={categoryFilter}
            onChange={setCategoryFilter}
          />
 
          {/* Status + View mode */}
          <div className="flex items-center gap-2">
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value as typeof statusFilter)}
              className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] px-3 py-2 text-sm text-[var(--text-primary)] focus:border-primary-500 focus:outline-none"
            >
              <option value="all">{t('products.allStatus')}</option>
              <option value="active">{t('common.active')}</option>
              <option value="inactive">{t('common.inactive')}</option>
            </select>
 
            {/* View toggle — hidden on mobile */}
            <div className="hidden sm:flex rounded-lg border border-[var(--border-color)] overflow-hidden">
              <button
                onClick={() => setViewMode('grid')}
                className={`p-2 ${viewMode === 'grid' ? 'bg-primary-600 text-white' : 'bg-[var(--bg-primary)] text-[var(--text-secondary)]'}`}
              >
                <LayoutGrid className="h-4 w-4" />
              </button>
              <button
                onClick={() => setViewMode('table')}
                className={`p-2 ${viewMode === 'table' ? 'bg-primary-600 text-white' : 'bg-[var(--bg-primary)] text-[var(--text-secondary)]'}`}
              >
                <List className="h-4 w-4" />
              </button>
            </div>
          </div>
        </div>
      </Card>
 
      {/* Content */}
      {isLoading ? (
        <Spinner />
      ) : !products?.length ? (
        <Card>
          <EmptyState
            icon={<Package className="h-12 w-12" />}
            title={t('products.noProductsFound')}
            description={t('products.noProductsDesc')}
            action={<Button onClick={() => setShowCreate(true)}>{t('products.createProduct')}</Button>}
          />
        </Card>
      ) : viewMode === 'grid' ? (
        /* Grid view */
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {products.map((product) => (
            <ProductCard
              key={product.id}
              product={product}
              onClick={() => setSelectedId(product.id)}
            />
          ))}
        </div>
      ) : (
        /* Table view */
        <Card>
          <div className="overflow-x-auto -mx-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('products.col_product')}</th>
                  <th className="hidden md:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('products.col_sku')}</th>
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('products.col_price')}</th>
                  <th className="hidden lg:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('products.col_category')}</th>
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('common.status')}</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('common.actions')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--border-color)]">
                {products.map((product) => (
                  <ProductRow
                    key={product.id}
                    product={product}
                    onViewDetail={() => setSelectedId(product.id)}
                  />
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}
 
      {/* Modals */}
      <CreateProductModal isOpen={showCreate} onClose={() => setShowCreate(false)} />
      {selectedId && (
        <ProductDetailPanel
          productId={selectedId}
          isOpen={!!selectedId}
          onClose={() => setSelectedId(null)}
        />
      )}
    </div>
  );
}