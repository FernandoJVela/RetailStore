import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, Plus, Package, LayoutGrid, List } from 'lucide-react';
import { Button, Card, Spinner, EmptyState } from '@shared/components/ui';
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
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-[var(--text-primary)]">{t('nav.products')}</h1>
          <p className="mt-1 text-sm text-[var(--text-secondary)]">
            {products?.length ?? 0} products in catalog
          </p>
        </div>
        <Button onClick={() => setShowCreate(true)}>
          <Plus className="h-4 w-4" />
          <span className="hidden sm:inline">Create Product</span>
          <span className="sm:hidden">New</span>
        </Button>
      </div>
 
      {/* Filters bar */}
      <Card>
        <div className="flex flex-col gap-4 lg:flex-row lg:items-center">
          {/* Search */}
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-[var(--text-muted)]" />
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search by name or SKU..."
              className="w-full rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] py-2.5 pl-10 pr-4 text-sm text-[var(--text-primary)] placeholder:text-[var(--text-muted)] focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
 
          {/* Category filter */}
          <div className="flex flex-wrap gap-2">
            <button
              onClick={() => setCategoryFilter('all')}
              className={`rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
                categoryFilter === 'all'
                  ? 'bg-primary-600 text-white'
                  : 'bg-[var(--bg-primary)] text-[var(--text-secondary)] hover:bg-[var(--bg-tertiary)]'
              }`}
            >
              All
            </button>
            {PRODUCT_CATEGORIES.map((cat) => (
              <button
                key={cat}
                onClick={() => setCategoryFilter(cat)}
                className={`rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
                  categoryFilter === cat
                    ? 'bg-primary-600 text-white'
                    : 'bg-[var(--bg-primary)] text-[var(--text-secondary)] hover:bg-[var(--bg-tertiary)]'
                }`}
              >
                {cat}
              </button>
            ))}
          </div>
 
          {/* Status + View mode */}
          <div className="flex items-center gap-2">
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value as typeof statusFilter)}
              className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] px-3 py-2 text-sm text-[var(--text-primary)] focus:border-primary-500 focus:outline-none"
            >
              <option value="all">All Status</option>
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
            title="No products found"
            description="Try adjusting your filters or create a new product."
            action={<Button onClick={() => setShowCreate(true)}>Create Product</Button>}
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
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Product</th>
                  <th className="hidden md:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">SKU</th>
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Price</th>
                  <th className="hidden lg:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Category</th>
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