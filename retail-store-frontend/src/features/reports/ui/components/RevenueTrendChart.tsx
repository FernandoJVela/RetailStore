import { useTranslation } from 'react-i18next';
import { Card, Spinner } from '@shared/components/ui';
import { useRevenueByPeriod } from '@features/reports/application/hooks/useReportsQueries';

export function RevenueTrendChart() {
  const { t } = useTranslation();
  const { data, isLoading } = useRevenueByPeriod(12);

  if (isLoading) return <Card title={t('reports.monthlyRevenue')}><Spinner /></Card>;
  if (!data?.length) return null;

  const maxRevenue = Math.max(...data.map((d) => d.revenue));

  return (
    <Card title={t('reports.monthlyRevenue')} subtitle={t('reports.last12Months')}>
      <div className="flex items-end gap-2 h-48">
        {data.map((item) => {
          const heightPercent = maxRevenue > 0 ? (item.revenue / maxRevenue) * 100 : 0;
          return (
            <div key={item.period} className="flex-1 flex flex-col items-center justify-end group">
              {/* Tooltip on hover */}
              <div className="hidden group-hover:block absolute -mt-16 rounded-lg bg-[var(--sidebar-bg)] text-white px-3 py-2 text-xs shadow-lg z-10">
                <p className="font-semibold">{item.formattedRevenue}</p>
                <p>{t('reports.ordersItems', { orders: item.orderCount, items: item.itemsSold })}</p>
              </div>
              {/* Bar */}
              <div
                className="w-full rounded-t-md bg-primary-500 hover:bg-primary-400 transition-all duration-300 cursor-pointer"
                style={{ height: `${Math.max(heightPercent, 2)}%` }}
              />
              {/* Label */}
              <span className="text-[10px] text-[var(--text-muted)] mt-1.5 tabular-nums">
                {item.period.split('-')[1]}
              </span>
            </div>
          );
        })}
      </div>
      {/* Y-axis labels */}
      <div className="flex justify-between mt-2 text-xs text-[var(--text-muted)]">
        <span>$0</span>
        <span>{data.length > 0 ? data.reduce((max, d) => d.revenue > max.revenue ? d : max, data[0]).formattedRevenue : ''}</span>
      </div>
    </Card>
  );
}
