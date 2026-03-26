namespace RetailStore.Api.Features.Reports.Application.DTOs;
 
// ─── Dashboard KPIs ─────────────────────────────────────────
public sealed record DashboardKpiDto(
    int TodayOrders, int WeekOrders, int MonthOrders,
    decimal TodayRevenue, decimal MonthRevenue,
    int ActiveProducts, int ActiveCustomers,
    int OutOfStockProducts, int LowStockProducts,
    int PendingShipments, int PendingPayments);
 
// ─── Sales Summary ──────────────────────────────────────────
public sealed record DailySalesDto(
    DateTime Date, int TotalOrders, int ConfirmedOrders,
    int CancelledOrders, int CompletedOrders,
    decimal TotalRevenue, int TotalItemsSold,
    decimal AverageOrderValue, int UniqueCustomers,
    string? TopCategory);
 
public sealed record SalesByCategoryDto(
    string Category, int OrderCount, int TotalQuantity,
    decimal TotalRevenue, decimal AveragePrice, int UniqueCustomers);
 
public sealed record RevenueByPeriodDto(
    int Year, int Month, string Period,
    int OrderCount, decimal Revenue,
    int UniqueCustomers, int ItemsSold);
 
// ─── Product Analytics ──────────────────────────────────────
public sealed record TopProductDto(
    Guid ProductId, string Name, string Sku, string Category,
    decimal CurrentPrice, string Currency,
    int TotalQuantitySold, decimal TotalRevenue,
    int OrderCount, decimal AverageSellingPrice);
 
// ─── Customer Analytics ─────────────────────────────────────
public sealed record CustomerSpendingDto(
    Guid CustomerId, string CustomerName, string Email, string? City,
    int TotalOrders, decimal TotalSpent, decimal AverageOrderValue,
    DateTime FirstOrderDate, DateTime LastOrderDate,
    int DaysSinceLastOrder);
 
// ─── Payment Analytics ──────────────────────────────────────
public sealed record PaymentMethodDto(
    string Method, int PaymentCount,
    decimal TotalAmount, decimal AverageAmount,
    int SuccessCount, int FailedCount, decimal SuccessRate);
 
// ─── Inventory Health ───────────────────────────────────────
public sealed record InventoryHealthDto(
    string Category, int ProductCount,
    int TotalStock, int TotalReserved, int TotalAvailable,
    int OutOfStockCount, int LowStockCount,
    decimal AvgStockPerProduct);
 
// ─── Shipping Performance ───────────────────────────────────
public sealed record ShippingPerformanceDto(
    string Carrier, int TotalShipments,
    int DeliveredCount, int FailedCount, int ReturnedCount,
    decimal AvgShippingCost, int? AvgDeliveryHours,
    decimal DeliverySuccessRate);