import type {
  DashboardKpiDto, DailySalesDto, SalesByCategoryDto,
  RevenueByPeriodDto, TopProductDto, CustomerSpendingDto,
  PaymentMethodDto, InventoryHealthDto, ShippingPerformanceDto,
} from '@features/reports';
import type {
  DashboardKpis, DailySales, SalesByCategory, RevenueByPeriod,
  TopProduct, CustomerSpending, PaymentMethodAnalytics,
  InventoryHealth, ShippingPerformance,
} from '@features/reports';
 
const PAYMENT_LABELS: Record<string, string> = {
  CreditCard: 'Credit Card', DebitCard: 'Debit Card',
  BankTransfer: 'Bank Transfer', Cash: 'Cash',
  DigitalWallet: 'Digital Wallet', PSE: 'PSE',
};
 
function fmt(amount: number, currency = 'USD'): string {
  try { return new Intl.NumberFormat('en-US', { style: 'currency', currency, minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(amount); }
  catch { return `$${Math.round(amount).toLocaleString()}`; }
}
 
function fmtFull(amount: number, currency = 'USD'): string {
  try { return new Intl.NumberFormat('en-US', { style: 'currency', currency, minimumFractionDigits: 2 }).format(amount); }
  catch { return `$${amount.toFixed(2)}`; }
}
 
export function mapDashboardKpis(dto: DashboardKpiDto): DashboardKpis {
  return {
    ...dto,
    formattedTodayRevenue: fmt(dto.todayRevenue),
    formattedMonthRevenue: fmt(dto.monthRevenue),
    totalAlerts: dto.outOfStockProducts + dto.lowStockProducts + dto.pendingShipments + dto.pendingPayments,
  };
}
 
export function mapDailySales(dto: DailySalesDto): DailySales {
  const d = new Date(dto.date);
  return {
    date: d,
    dateLabel: d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
    totalOrders: dto.totalOrders,
    totalRevenue: dto.totalRevenue,
    averageOrderValue: dto.averageOrderValue,
    uniqueCustomers: dto.uniqueCustomers,
    topCategory: dto.topCategory,
  };
}
 
export function mapSalesByCategory(dto: SalesByCategoryDto): SalesByCategory {
  return { ...dto, formattedRevenue: fmt(dto.totalRevenue) };
}
 
export function mapRevenueByPeriod(dto: RevenueByPeriodDto): RevenueByPeriod {
  return { ...dto, formattedRevenue: fmt(dto.revenue) };
}
 
export function mapTopProduct(dto: TopProductDto): TopProduct {
  return {
    productId: dto.productId, name: dto.name, sku: dto.sku, category: dto.category,
    currentPrice: dto.currentPrice, formattedPrice: fmtFull(dto.currentPrice, dto.currency),
    totalQuantitySold: dto.totalQuantitySold, totalRevenue: dto.totalRevenue,
    formattedRevenue: fmt(dto.totalRevenue), orderCount: dto.orderCount,
  };
}
 
export function mapCustomerSpending(dto: CustomerSpendingDto): CustomerSpending {
  return {
    customerId: dto.customerId, customerName: dto.customerName,
    email: dto.email, city: dto.city,
    totalOrders: dto.totalOrders, totalSpent: dto.totalSpent,
    formattedSpent: fmt(dto.totalSpent), averageOrderValue: dto.averageOrderValue,
    daysSinceLastOrder: dto.daysSinceLastOrder,
    isChurning: dto.daysSinceLastOrder > 60,
  };
}
 
export function mapPaymentMethod(dto: PaymentMethodDto): PaymentMethodAnalytics {
  return {
    method: dto.method, methodLabel: PAYMENT_LABELS[dto.method] ?? dto.method,
    paymentCount: dto.paymentCount, totalAmount: dto.totalAmount,
    formattedTotal: fmt(dto.totalAmount), successRate: dto.successRate,
    failedCount: dto.failedCount,
  };
}
 
export function mapInventoryHealth(dto: InventoryHealthDto): InventoryHealth {
  const healthy = dto.productCount - dto.outOfStockCount - dto.lowStockCount;
  return {
    category: dto.category, productCount: dto.productCount,
    totalStock: dto.totalStock, totalAvailable: dto.totalAvailable,
    outOfStockCount: dto.outOfStockCount, lowStockCount: dto.lowStockCount,
    healthPercent: dto.productCount > 0 ? Math.round((healthy / dto.productCount) * 100) : 0,
  };
}
 
export function mapShippingPerformance(dto: ShippingPerformanceDto): ShippingPerformance {
  return {
    carrier: dto.carrier, totalShipments: dto.totalShipments,
    deliveredCount: dto.deliveredCount, failedCount: dto.failedCount,
    deliverySuccessRate: dto.deliverySuccessRate,
    avgDeliveryHours: dto.avgDeliveryHours,
    avgDeliveryDays: dto.avgDeliveryHours ? (dto.avgDeliveryHours / 24).toFixed(1) + ' days' : null,
    avgShippingCost: dto.avgShippingCost, formattedCost: fmtFull(dto.avgShippingCost),
  };
}