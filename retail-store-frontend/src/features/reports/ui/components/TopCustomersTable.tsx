import { Card, Badge, Spinner } from '@shared/components/ui';
import { useTopCustomers } from '@features/reports/application/hooks/useReportsQueries';
 
export function TopCustomersTable() {
  const { data, isLoading } = useTopCustomers(10);
 
  if (isLoading) return <Card title="Top Customers"><Spinner /></Card>;
  if (!data?.length) return null;
 
  return (
    <Card title="Top Spending Customers" subtitle="Lifetime value ranking">
      <div className="overflow-x-auto -mx-6">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-[var(--border-color)]">
              <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">#</th>
              <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Customer</th>
              <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">Orders</th>
              <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">Total Spent</th>
              <th className="hidden md:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">Last Order</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-[var(--border-color)]">
            {data.map((customer, idx) => (
              <tr key={customer.customerId} className="hover:bg-[var(--bg-tertiary)]/50 transition-colors">
                <td className="px-6 py-3 text-[var(--text-muted)] tabular-nums">{idx + 1}</td>
                <td className="px-6 py-3">
                  <div className="flex items-center gap-2">
                    <div className="min-w-0">
                      <p className="font-medium text-[var(--text-primary)] truncate">{customer.customerName}</p>
                      <p className="text-xs text-[var(--text-muted)]">{customer.city ?? customer.email}</p>
                    </div>
                    {customer.isChurning && <Badge variant="warning">At risk</Badge>}
                  </div>
                </td>
                <td className="px-6 py-3 text-right tabular-nums text-[var(--text-primary)]">
                  {customer.totalOrders}
                </td>
                <td className="px-6 py-3 text-right font-semibold tabular-nums text-[var(--text-primary)]">
                  {customer.formattedSpent}
                </td>
                <td className="hidden md:table-cell px-6 py-3 text-right text-[var(--text-secondary)]">
                  {customer.daysSinceLastOrder}d ago
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Card>
  );
}