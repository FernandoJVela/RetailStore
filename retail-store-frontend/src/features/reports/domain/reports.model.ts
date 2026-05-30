/** Domain models — what the UI actually needs for reporting. */
 
export interface DashboardKpis {
  todayOrders: number;
  weekOrders: number;
  monthOrders: number;
  todayRevenue: number;
  monthRevenue: number;
  formattedTodayRevenue: string;
  formattedMonthRevenue: string;
  activeProducts: number;
  activeCustomers: number;
  outOfStockProducts: number;
  lowStockProducts: number;
  pendingShipments: number;
  pendingPayments: number;
  totalAlerts: number; // Computed: outOfStock + lowStock + pendingShipments + pendingPayments
}
 
export interface DailySales {
  date: Date;
  dateLabel: string;
  totalOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
  uniqueCustomers: number;
  topCategory: string | null;
}
 
export interface SalesByCategory {
  category: string;
  orderCount: number;
  totalQuantity: number;
  totalRevenue: number;
  formattedRevenue: string;
  averagePrice: number;
  uniqueCustomers: number;
}
 
export interface RevenueByPeriod {
  period: string;
  year: number;
  month: number;
  orderCount: number;
  revenue: number;
  formattedRevenue: string;
  uniqueCustomers: number;
  itemsSold: number;
}
 
export interface TopProduct {
  productId: string;
  name: string;
  sku: string;
  category: string;
  currentPrice: number;
  formattedPrice: string;
  totalQuantitySold: number;
  totalRevenue: number;
  formattedRevenue: string;
  orderCount: number;
}
 
export interface CustomerSpending {
  customerId: string;
  customerName: string;
  email: string;
  city: string | null;
  totalOrders: number;
  totalSpent: number;
  formattedSpent: string;
  averageOrderValue: number;
  daysSinceLastOrder: number;
  isChurning: boolean; // Computed: > 60 days since last order
}
 
export interface PaymentMethodAnalytics {
  method: string;
  methodLabel: string;
  paymentCount: number;
  totalAmount: number;
  formattedTotal: string;
  successRate: number;
  failedCount: number;
}
 
export interface InventoryHealth {
  category: string;
  productCount: number;
  totalStock: number;
  totalAvailable: number;
  outOfStockCount: number;
  lowStockCount: number;
  healthPercent: number; // Computed: (productCount - outOfStock - lowStock) / productCount
}
 
export interface ShippingPerformance {
  carrier: string;
  totalShipments: number;
  deliveredCount: number;
  failedCount: number;
  deliverySuccessRate: number;
  avgDeliveryHours: number | null;
  avgDeliveryDays: string | null; // Computed: hours → "X.X days"
  avgShippingCost: number;
  formattedCost: string;
}