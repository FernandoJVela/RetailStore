/** API DTOs — match backend JSON responses exactly. */
 
export interface DashboardKpiDto {
  todayOrders: number;
  weekOrders: number;
  monthOrders: number;
  todayRevenue: number;
  monthRevenue: number;
  activeProducts: number;
  activeCustomers: number;
  outOfStockProducts: number;
  lowStockProducts: number;
  pendingShipments: number;
  pendingPayments: number;
}
 
export interface DailySalesDto {
  date: string;
  totalOrders: number;
  confirmedOrders: number;
  cancelledOrders: number;
  completedOrders: number;
  totalRevenue: number;
  totalItemsSold: number;
  averageOrderValue: number;
  uniqueCustomers: number;
  topCategory: string | null;
}
 
export interface SalesByCategoryDto {
  category: string;
  orderCount: number;
  totalQuantity: number;
  totalRevenue: number;
  averagePrice: number;
  uniqueCustomers: number;
}
 
export interface RevenueByPeriodDto {
  year: number;
  month: number;
  period: string;
  orderCount: number;
  revenue: number;
  uniqueCustomers: number;
  itemsSold: number;
}
 
export interface TopProductDto {
  productId: string;
  name: string;
  sku: string;
  category: string;
  currentPrice: number;
  currency: string;
  totalQuantitySold: number;
  totalRevenue: number;
  orderCount: number;
  averageSellingPrice: number;
}
 
export interface CustomerSpendingDto {
  customerId: string;
  customerName: string;
  email: string;
  city: string | null;
  totalOrders: number;
  totalSpent: number;
  averageOrderValue: number;
  firstOrderDate: string;
  lastOrderDate: string;
  daysSinceLastOrder: number;
}
 
export interface PaymentMethodDto {
  method: string;
  paymentCount: number;
  totalAmount: number;
  averageAmount: number;
  successCount: number;
  failedCount: number;
  successRate: number;
}
 
export interface InventoryHealthDto {
  category: string;
  productCount: number;
  totalStock: number;
  totalReserved: number;
  totalAvailable: number;
  outOfStockCount: number;
  lowStockCount: number;
  avgStockPerProduct: number;
}
 
export interface ShippingPerformanceDto {
  carrier: string;
  totalShipments: number;
  deliveredCount: number;
  failedCount: number;
  returnedCount: number;
  avgShippingCost: number;
  avgDeliveryHours: number | null;
  deliverySuccessRate: number;
}