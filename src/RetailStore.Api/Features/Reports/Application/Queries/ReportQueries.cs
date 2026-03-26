using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Customers.Domain;
using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Payments.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.Api.Features.Reports.Application.DTOs;
using RetailStore.Api.Features.Reports.Domain;
using RetailStore.Api.Features.Shipping.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
 
namespace RetailStore.Api.Features.Reports.Application.Queries;
 
// ═══════════════════════════════════════════════════════════
// DASHBOARD KPIs (real-time, single call for overview)
// ═══════════════════════════════════════════════════════════
public sealed record GetDashboardKpisQuery() : IQuery<DashboardKpiDto>;
 
public sealed class GetDashboardKpisHandler(RetailStoreDbContext db)
    : IRequestHandler<GetDashboardKpisQuery, DashboardKpiDto>
{
    public async Task<DashboardKpiDto> Handle(GetDashboardKpisQuery q, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);
 
        var validStatuses = new[] { "Confirmed", "Shipped", "Delivered", "Completed" };
 
        var orders = db.Set<Order>().AsNoTracking();
        var activeOrders = orders.Where(o => validStatuses.Contains(o.Status.ToString()));
 
        var todayOrders = await activeOrders.CountAsync(o => o.OrderDate >= today, ct);
        var weekOrders = await activeOrders.CountAsync(o => o.OrderDate >= weekAgo, ct);
        var monthOrders = await activeOrders.CountAsync(o => o.OrderDate >= monthAgo, ct);
 
        // Revenue from line items
        var todayRevenue = await db.Set<OrderItem>().AsNoTracking()
            .Join(activeOrders.Where(o => o.OrderDate >= today),
                li => li.OrderId, o => o.Id, (li, o) => li)
            .SumAsync(li => li.UnitPrice * li.Quantity, ct);
 
        var monthRevenue = await db.Set<OrderItem>().AsNoTracking()
            .Join(activeOrders.Where(o => o.OrderDate >= monthAgo),
                li => li.OrderId, o => o.Id, (li, o) => li)
            .SumAsync(li => li.UnitPrice * li.Quantity, ct);
 
        var activeProducts = await db.Set<Product>().AsNoTracking()
            .CountAsync(p => p.IsActive, ct);
        var activeCustomers = await db.Set<Customer>().AsNoTracking()
            .CountAsync(c => c.IsActive, ct);
 
        // Inventory alerts
        var inventoryItems = await db.Set<InventoryItem>().AsNoTracking().ToListAsync(ct);
        var outOfStock = inventoryItems.Count(i => (i.QuantityOnHand - i.ReservedQuantity) <= 0);
        var lowStock = inventoryItems.Count(i => i.QuantityOnHand <= i.ReorderThreshold
            && (i.QuantityOnHand - i.ReservedQuantity) > 0);
 
        var pendingShipments = await db.Set<Shipment>().AsNoTracking()
            .CountAsync(s => s.Status == ShipmentStatus.Pending || s.Status == ShipmentStatus.Processing, ct);
        var pendingPayments = await db.Set<Payment>().AsNoTracking()
            .CountAsync(p => p.Status == PaymentStatus.Pending, ct);
 
        return new DashboardKpiDto(
            todayOrders, weekOrders, monthOrders,
            todayRevenue, monthRevenue,
            activeProducts, activeCustomers,
            outOfStock, lowStock,
            pendingShipments, pendingPayments);
    }
}
 
// ═══════════════════════════════════════════════════════════
// SALES BY CATEGORY
// ═══════════════════════════════════════════════════════════
public sealed record GetSalesByCategoryQuery() : IQuery<List<SalesByCategoryDto>>;
 
public sealed class GetSalesByCategoryHandler(RetailStoreDbContext db)
    : IRequestHandler<GetSalesByCategoryQuery, List<SalesByCategoryDto>>
{
    public async Task<List<SalesByCategoryDto>> Handle(GetSalesByCategoryQuery q, CancellationToken ct)
    {
        var validStatuses = new[] { "Confirmed", "Shipped", "Delivered", "Completed" };
 
        var results = await (
            from li in db.Set<OrderItem>().AsNoTracking()
            join o in db.Set<Order>().AsNoTracking() on li.OrderId equals o.Id
            join p in db.Set<Product>().AsNoTracking() on li.ProductId equals p.Id
            where validStatuses.Contains(o.Status.ToString())
            group new { li, o } by p.Category into g
            select new
            {
                Category = g.Key,
                OrderCount = g.Select(x => x.o.Id).Distinct().Count(),
                TotalQuantity = g.Sum(x => x.li.Quantity),
                TotalRevenue = g.Sum(x => x.li.UnitPrice * x.li.Quantity),
                AveragePrice = g.Average(x => x.li.UnitPrice),
                UniqueCustomers = g.Select(x => x.o.CustomerId).Distinct().Count()
            }
        ).ToListAsync(ct);
 
        return results.Select(r => new SalesByCategoryDto(
            r.Category, r.OrderCount, r.TotalQuantity,
            r.TotalRevenue, r.AveragePrice, r.UniqueCustomers))
            .OrderByDescending(r => r.TotalRevenue).ToList();
    }
}
 
// ═══════════════════════════════════════════════════════════
// TOP SELLING PRODUCTS
// ═══════════════════════════════════════════════════════════
public sealed record GetTopProductsQuery(int Top = 10) : IQuery<List<TopProductDto>>;
 
public sealed class GetTopProductsHandler(RetailStoreDbContext db)
    : IRequestHandler<GetTopProductsQuery, List<TopProductDto>>
{
    public async Task<List<TopProductDto>> Handle(GetTopProductsQuery q, CancellationToken ct)
    {
        var validStatuses = new[] { "Confirmed", "Shipped", "Delivered", "Completed" };
 
        var results = await (
            from li in db.Set<OrderItem>().AsNoTracking()
            join o in db.Set<Order>().AsNoTracking() on li.OrderId equals o.Id
            join p in db.Set<Product>().AsNoTracking() on li.ProductId equals p.Id
            where validStatuses.Contains(o.Status.ToString())
            group li by new { p.Id, p.Name, p.Sku, p.Category } into g
            select new
            {
                g.Key.Id, g.Key.Name, g.Key.Sku, g.Key.Category,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.UnitPrice * x.Quantity),
                OrderCount = g.Select(x => x.OrderId).Distinct().Count(),
                AvgPrice = g.Average(x => x.UnitPrice)
            }
        ).ToListAsync(ct);
 
        // Get current prices in memory (ComplexProperty can't project)
        var productIds = results.Select(r => r.Id).ToList();
        var prices = await db.Set<Product>().AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);
        var priceMap = prices.ToDictionary(p => p.Id, p => (p.Price.Amount, p.Price.Currency));
 
        return results
            .OrderByDescending(r => r.TotalRevenue)
            .Take(q.Top)
            .Select(r => new TopProductDto(
                r.Id, r.Name, r.Sku, r.Category,
                priceMap.TryGetValue(r.Id, out var price) ? price.Amount : 0,
                priceMap.TryGetValue(r.Id, out var curr) ? curr.Currency : "USD",
                r.TotalQuantity, r.TotalRevenue,
                r.OrderCount, r.AvgPrice))
            .ToList();
    }
}
 
// ═══════════════════════════════════════════════════════════
// REVENUE BY PERIOD (monthly)
// ═══════════════════════════════════════════════════════════
public sealed record GetRevenueByPeriodQuery(int Months = 12) : IQuery<List<RevenueByPeriodDto>>;
 
public sealed class GetRevenueByPeriodHandler(RetailStoreDbContext db)
    : IRequestHandler<GetRevenueByPeriodQuery, List<RevenueByPeriodDto>>
{
    public async Task<List<RevenueByPeriodDto>> Handle(GetRevenueByPeriodQuery q, CancellationToken ct)
    {
        var since = DateTime.UtcNow.AddMonths(-q.Months);
        var validStatuses = new[] { "Confirmed", "Shipped", "Delivered", "Completed" };
 
        var data = await (
            from li in db.Set<OrderItem>().AsNoTracking()
            join o in db.Set<Order>().AsNoTracking() on li.OrderId equals o.Id
            where validStatuses.Contains(o.Status.ToString()) && o.OrderDate >= since
            group new { li, o } by new { o.OrderDate.Year, o.OrderDate.Month } into g
            select new
            {
                g.Key.Year, g.Key.Month,
                OrderCount = g.Select(x => x.o.Id).Distinct().Count(),
                Revenue = g.Sum(x => x.li.UnitPrice * x.li.Quantity),
                UniqueCustomers = g.Select(x => x.o.CustomerId).Distinct().Count(),
                ItemsSold = g.Sum(x => x.li.Quantity)
            }
        ).ToListAsync(ct);
 
        return data.OrderBy(d => d.Year).ThenBy(d => d.Month)
            .Select(d => new RevenueByPeriodDto(
                d.Year, d.Month, $"{d.Year:D4}-{d.Month:D2}",
                d.OrderCount, d.Revenue, d.UniqueCustomers, d.ItemsSold))
            .ToList();
    }
}
 
// ═══════════════════════════════════════════════════════════
// CUSTOMER SPENDING (top customers)
// ═══════════════════════════════════════════════════════════
public sealed record GetTopCustomersQuery(int Top = 20) : IQuery<List<CustomerSpendingDto>>;
 
public sealed class GetTopCustomersHandler(RetailStoreDbContext db)
    : IRequestHandler<GetTopCustomersQuery, List<CustomerSpendingDto>>
{
    public async Task<List<CustomerSpendingDto>> Handle(GetTopCustomersQuery q, CancellationToken ct)
    {
        var validStatuses = new[] { "Confirmed", "Shipped", "Delivered", "Completed" };
 
        var data = await (
            from o in db.Set<Order>().AsNoTracking()
            join c in db.Set<Customer>().AsNoTracking() on o.CustomerId equals c.Id
            join li in db.Set<OrderItem>().AsNoTracking() on o.Id equals li.OrderId
            where validStatuses.Contains(o.Status.ToString())
            group new { o, li } by new { c.Id, c.FirstName, c.LastName, c.Email, c.ShippingCity } into g
            select new
            {
                g.Key.Id, g.Key.FirstName, g.Key.LastName, g.Key.Email, g.Key.ShippingCity,
                TotalOrders = g.Select(x => x.o.Id).Distinct().Count(),
                TotalSpent = g.Sum(x => x.li.UnitPrice * x.li.Quantity),
                FirstOrder = g.Min(x => x.o.OrderDate),
                LastOrder = g.Max(x => x.o.OrderDate)
            }
        ).ToListAsync(ct);
 
        return data.OrderByDescending(d => d.TotalSpent).Take(q.Top)
            .Select(d => new CustomerSpendingDto(
                d.Id, $"{d.FirstName} {d.LastName}", d.Email, d.ShippingCity,
                d.TotalOrders, d.TotalSpent,
                d.TotalOrders > 0 ? d.TotalSpent / d.TotalOrders : 0,
                d.FirstOrder, d.LastOrder,
                (DateTime.UtcNow - d.LastOrder).Days))
            .ToList();
    }
}
 
// ═══════════════════════════════════════════════════════════
// PAYMENT METHOD DISTRIBUTION
// ═══════════════════════════════════════════════════════════
public sealed record GetPaymentAnalyticsQuery() : IQuery<List<PaymentMethodDto>>;
 
public sealed class GetPaymentAnalyticsHandler(RetailStoreDbContext db)
    : IRequestHandler<GetPaymentAnalyticsQuery, List<PaymentMethodDto>>
{
    public async Task<List<PaymentMethodDto>> Handle(GetPaymentAnalyticsQuery q, CancellationToken ct)
    {
        var payments = await db.Set<Payment>().AsNoTracking().ToListAsync(ct);
 
        return payments.GroupBy(p => p.Method.ToString())
            .Select(g => new PaymentMethodDto(
                g.Key,
                g.Count(),
                g.Sum(p => p.Amount),
                g.Average(p => p.Amount),
                g.Count(p => p.Status == PaymentStatus.Captured),
                g.Count(p => p.Status == PaymentStatus.Failed),
                g.Count() > 0
                    ? Math.Round(g.Count(p => p.Status == PaymentStatus.Captured) * 100m / g.Count(), 2)
                    : 0))
            .OrderByDescending(m => m.TotalAmount)
            .ToList();
    }
}
 
// ═══════════════════════════════════════════════════════════
// INVENTORY HEALTH (by category)
// ═══════════════════════════════════════════════════════════
public sealed record GetInventoryHealthQuery() : IQuery<List<InventoryHealthDto>>;
 
public sealed class GetInventoryHealthHandler(RetailStoreDbContext db)
    : IRequestHandler<GetInventoryHealthQuery, List<InventoryHealthDto>>
{
    public async Task<List<InventoryHealthDto>> Handle(GetInventoryHealthQuery q, CancellationToken ct)
    {
        var data = await (
            from i in db.Set<InventoryItem>().AsNoTracking()
            join p in db.Set<Product>().AsNoTracking() on i.ProductId equals p.Id
            where p.IsActive
            select new { p.Category, i.QuantityOnHand, i.ReservedQuantity, i.ReorderThreshold }
        ).ToListAsync(ct);
 
        return data.GroupBy(d => d.Category)
            .Select(g => new InventoryHealthDto(
                g.Key,
                g.Count(),
                g.Sum(x => x.QuantityOnHand),
                g.Sum(x => x.ReservedQuantity),
                g.Sum(x => x.QuantityOnHand - x.ReservedQuantity),
                g.Count(x => (x.QuantityOnHand - x.ReservedQuantity) <= 0),
                g.Count(x => x.QuantityOnHand <= x.ReorderThreshold && (x.QuantityOnHand - x.ReservedQuantity) > 0),
                Math.Round(g.Average(x => (decimal)x.QuantityOnHand), 2)))
            .OrderBy(h => h.Category)
            .ToList();
    }
}
 
// ═══════════════════════════════════════════════════════════
// SHIPPING PERFORMANCE (by carrier)
// ═══════════════════════════════════════════════════════════
public sealed record GetShippingPerformanceQuery() : IQuery<List<ShippingPerformanceDto>>;
 
public sealed class GetShippingPerformanceHandler(RetailStoreDbContext db)
    : IRequestHandler<GetShippingPerformanceQuery, List<ShippingPerformanceDto>>
{
    public async Task<List<ShippingPerformanceDto>> Handle(GetShippingPerformanceQuery q, CancellationToken ct)
    {
        var shipments = await db.Set<Shipment>().AsNoTracking()
            .Where(s => s.Carrier != null)
            .ToListAsync(ct);
 
        return shipments.GroupBy(s => s.Carrier!)
            .Select(g =>
            {
                var deliveredWithTimes = g.Where(s => s.DeliveredAt.HasValue && s.ShippedAt.HasValue).ToList();
                var avgHours = deliveredWithTimes.Any()
                    ? (int?)deliveredWithTimes.Average(s => (s.DeliveredAt!.Value - s.ShippedAt!.Value).TotalHours)
                    : null;
 
                return new ShippingPerformanceDto(
                    g.Key,
                    g.Count(),
                    g.Count(s => s.Status == ShipmentStatus.Delivered),
                    g.Count(s => s.Status == ShipmentStatus.Failed),
                    g.Count(s => s.Status == ShipmentStatus.Returned),
                    Math.Round(g.Average(s => s.ShippingCost), 2),
                    avgHours,
                    g.Count() > 0
                        ? Math.Round(g.Count(s => s.Status == ShipmentStatus.Delivered) * 100m / g.Count(), 2)
                        : 0);
            })
            .OrderByDescending(c => c.TotalShipments)
            .ToList();
    }
}
 
// ═══════════════════════════════════════════════════════════
// DAILY SALES HISTORY (from snapshot table)
// ═══════════════════════════════════════════════════════════
public sealed record GetDailySalesQuery(int Days = 30) : IQuery<List<DailySalesDto>>;
 
public sealed class GetDailySalesHandler(RetailStoreDbContext db)
    : IRequestHandler<GetDailySalesQuery, List<DailySalesDto>>
{
    public async Task<List<DailySalesDto>> Handle(GetDailySalesQuery q, CancellationToken ct)
    {
        var since = DateTime.UtcNow.Date.AddDays(-q.Days);
 
        return await db.Set<DailySalesSnapshot>()
            .AsNoTracking()
            .Where(s => s.Date >= DateOnly.FromDateTime(since))
            .OrderByDescending(s => s.Date)
            .Select(s => new DailySalesDto(
                s.Date.ToDateTime(TimeOnly.MinValue),
                s.TotalOrders, s.ConfirmedOrders, s.CancelledOrders, s.CompletedOrders,
                s.TotalRevenue, s.TotalItemsSold, s.AverageOrderValue,
                s.UniqueCustomers, s.TopCategory))
            .ToListAsync(ct);
    }
}