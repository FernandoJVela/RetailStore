using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Infrastructure.Persistence;

namespace RetailStore.Api.Features.Inventory.Application.Events;

// ─── Inventory-specific event handlers ──────────────────────
public sealed class LowStockAlertHandler(ILogger<LowStockAlertHandler> log)
    : INotificationHandler<LowStockAlertEvent>
{
    public Task Handle(LowStockAlertEvent e, CancellationToken ct)
    {
        log.LogWarning(
            "LOW STOCK: Product {ProductId} has {Quantity} units (threshold: {Threshold})",
            e.ProductId, e.CurrentQuantity, e.Threshold);
        return Task.CompletedTask;
    }
}

public sealed class OutOfStockAlertHandler(ILogger<OutOfStockAlertHandler> log)
    : INotificationHandler<OutOfStockAlertEvent>
{
    public Task Handle(OutOfStockAlertEvent e, CancellationToken ct)
    {
        log.LogError(
            "OUT OF STOCK: Product {ProductId} has no available units!",
            e.ProductId);
        return Task.CompletedTask;
    }
}

public sealed class StockRecoveredHandler(ILogger<StockRecoveredHandler> log)
    : INotificationHandler<StockRecoveredEvent>
{
    public Task Handle(StockRecoveredEvent e, CancellationToken ct)
    {
        log.LogInformation(
            "STOCK RECOVERED: Product {ProductId} now has {Quantity} units",
            e.ProductId, e.CurrentQuantity);
        return Task.CompletedTask;
    }
}

// ─── Cross-module: Reserve stock when order is confirmed ────
public sealed class ReserveStockOnOrderConfirmedHandler(
    IInventoryRepository inventory,
    RetailStoreDbContext db,
    ILogger<ReserveStockOnOrderConfirmedHandler> log)
    : INotificationHandler<OrderConfirmedEvent>
{
    public async Task Handle(OrderConfirmedEvent e, CancellationToken ct)
    {
        var order = await db.Set<Order>()
            .Include(o => o.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == e.OrderId, ct);

        if (order is null)
        {
            log.LogWarning("ReserveStock: order {OrderId} not found", e.OrderId);
            return;
        }

        foreach (var item in order.Items)
        {
            var inventoryItem = await inventory.GetByProductIdAsync(item.ProductId, ct);
            if (inventoryItem is null)
            {
                log.LogWarning(
                    "No inventory record for product {ProductId} — skipping reservation",
                    item.ProductId);
                continue;
            }

            try
            {
                inventoryItem.Reserve(item.Quantity);
                inventory.Update(inventoryItem);
                log.LogInformation(
                    "Reserved {Qty} units of product {ProductId} for order {OrderId}",
                    item.Quantity, item.ProductId, e.OrderId);
            }
            catch (Exception ex)
            {
                log.LogError(ex,
                    "Failed to reserve {Qty} units of product {ProductId} for order {OrderId}",
                    item.Quantity, item.ProductId, e.OrderId);
            }
        }
    }
}

// ─── Cross-module: Release reservations when order is cancelled ─
public sealed class ReleaseStockOnOrderCancelledHandler(
    IInventoryRepository inventory,
    RetailStoreDbContext db,
    ILogger<ReleaseStockOnOrderCancelledHandler> log)
    : INotificationHandler<OrderCancelledEvent>
{
    public async Task Handle(OrderCancelledEvent e, CancellationToken ct)
    {
        var order = await db.Set<Order>()
            .Include(o => o.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == e.OrderId, ct);

        if (order is null)
        {
            log.LogWarning("ReleaseStock: order {OrderId} not found", e.OrderId);
            return;
        }

        foreach (var item in order.Items)
        {
            var inventoryItem = await inventory.GetByProductIdAsync(item.ProductId, ct);
            if (inventoryItem is null)
                continue;

            try
            {
                inventoryItem.ReleaseReservation(item.Quantity);
                inventory.Update(inventoryItem);
                log.LogInformation(
                    "Released {Qty} units reservation for product {ProductId} (order {OrderId} cancelled)",
                    item.Quantity, item.ProductId, e.OrderId);
            }
            catch (Exception ex)
            {
                log.LogError(ex,
                    "Failed to release reservation for product {ProductId} on order {OrderId} cancellation",
                    item.ProductId, e.OrderId);
            }
        }
    }
}

// ─── Cross-module: Fulfill reservations when order is completed ─
public sealed class FulfillStockOnOrderCompletedHandler(
    IInventoryRepository inventory,
    RetailStoreDbContext db,
    ILogger<FulfillStockOnOrderCompletedHandler> log)
    : INotificationHandler<OrderCompletedEvent>
{
    public async Task Handle(OrderCompletedEvent e, CancellationToken ct)
    {
        var order = await db.Set<Order>()
            .Include(o => o.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == e.OrderId, ct);

        if (order is null)
        {
            log.LogWarning("FulfillStock: order {OrderId} not found", e.OrderId);
            return;
        }

        foreach (var item in order.Items)
        {
            var inventoryItem = await inventory.GetByProductIdAsync(item.ProductId, ct);
            if (inventoryItem is null)
                continue;

            try
            {
                inventoryItem.FulfillReservation(item.Quantity);
                inventory.Update(inventoryItem);
                log.LogInformation(
                    "Fulfilled {Qty} units for product {ProductId} (order {OrderId} completed)",
                    item.Quantity, item.ProductId, e.OrderId);
            }
            catch (Exception ex)
            {
                log.LogError(ex,
                    "Failed to fulfill inventory for product {ProductId} on order {OrderId} completion",
                    item.ProductId, e.OrderId);
            }
        }
    }
}
