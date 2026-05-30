import { Card, Spinner } from '@shared/components/ui';
import { cn } from '@shared/lib/utils';
import { useSalesByCategory } from '@features/reports/application/hooks/useReportsQueries';
 
const COLORS = ['bg-primary-500', 'bg-emerald-500', 'bg-amber-500', 'bg-violet-500', 'bg-rose-500', 'bg-cyan-500'];
 
export function SalesByCategoryChart() {
  const { data, isLoading } = useSalesByCategory();
 
  if (isLoading) return <Card title="Sales by Category"><Spinner /></Card>;
  if (!data?.length) return null;
 
  const maxRevenue = Math.max(...data.map((d) => d.totalRevenue));
 
  return (
    <Card title="Sales by Category" subtitle="Revenue distribution across product categories">
      <div className="space-y-4">
        {data.map((item, idx) => (
          <div key={item.category}>
            <div className="flex items-center justify-between mb-1.5">
              <span className="text-sm font-medium text-[var(--text-primary)]">{item.category}</span>
              <span className="text-sm font-semibold text-[var(--text-primary)] tabular-nums">{item.formattedRevenue}</span>
            </div>
            <div className="h-3 w-full rounded-full bg-[var(--bg-tertiary)] overflow-hidden">
              <div
                className={cn('h-full rounded-full transition-all duration-500', COLORS[idx % COLORS.length])}
                style={{ width: `${(item.totalRevenue / maxRevenue) * 100}%` }}
              />
            </div>
            <div className="flex gap-4 mt-1 text-xs text-[var(--text-muted)]">
              <span>{item.orderCount} orders</span>
              <span>{item.totalQuantity} items</span>
              <span>{item.uniqueCustomers} customers</span>
            </div>
          </div>
        ))}
      </div>
    </Card>
  );
}