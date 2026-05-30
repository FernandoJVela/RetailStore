import { Card, Badge, Spinner } from '@shared/components/ui';
import { useTopProducts } from '@features/reports/application/hooks/useReportsQueries';
 
export function TopProductsTable() {
  const { data, isLoading } = useTopProducts(10);
 
  if (isLoading) return <Card title="Top Products"><Spinner /></Card>;
  if (!data?.length) return null;
 
  return (
    <Card title="Top Selling Products" subtitle="By total revenue">
      <div className="overflow-x-auto -mx-6">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-[var(--border-color)]">
              <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">#</th>
              <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Product</th>
              <th className="hidden md:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Category</th>
              <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">Qty Sold</th>
              <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">Revenue</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-[var(--border-color)]">
            {data.map((product, idx) => (
              <tr key={product.productId} className="hover:bg-[var(--bg-tertiary)]/50 transition-colors">
                <td className="px-6 py-3 text-[var(--text-muted)] tabular-nums">{idx + 1}</td>
                <td className="px-6 py-3">
                  <p className="font-medium text-[var(--text-primary)] truncate">{product.name}</p>
                  <p className="text-xs text-[var(--text-muted)] font-mono">{product.sku}</p>
                </td>
                <td className="hidden md:table-cell px-6 py-3">
                  <Badge variant="default">{product.category}</Badge>
                </td>
                <td className="px-6 py-3 text-right tabular-nums text-[var(--text-primary)]">
                  {product.totalQuantitySold.toLocaleString()}
                </td>
                <td className="px-6 py-3 text-right font-semibold tabular-nums text-[var(--text-primary)]">
                  {product.formattedRevenue}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Card>
  );
}