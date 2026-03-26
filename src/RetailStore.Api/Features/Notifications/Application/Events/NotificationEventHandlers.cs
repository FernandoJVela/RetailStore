using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.Api.Features.Notifications.Domain;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Shipping.Domain;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Infrastructure.Persistence;
 
namespace RetailStore.Api.Features.Notifications.Application.Events;
 
// ─── Order Events → Notifications ───────────────────────────
public sealed class NotifyOnOrderConfirmed(RetailStoreDbContext db, ILogger<NotifyOnOrderConfirmed> log)
    : INotificationHandler<OrderConfirmedEvent>
{
    public async Task Handle(OrderConfirmedEvent e, CancellationToken ct)
    {
        var template = await db.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Name == "OrderConfirmed"
                && t.Channel == NotificationChannel.InApp && t.IsActive, ct);
 
        if (template is null) { log.LogWarning("Template OrderConfirmed/InApp not found"); return; }
 
        var variables = new Dictionary<string, string>
        {
            ["OrderId"] = e.OrderId.ToString()[..8],
            ["TotalAmount"] = e.TotalAmount.ToString("F2"),
            ["Currency"] = "USD"
        };
 
        var notification = Notification.CreateFromTemplate(
            template, variables, e.CustomerId, RecipientType.Customer,
            referenceType: "Order", referenceId: e.OrderId);
 
        await db.Set<Notification>().AddAsync(notification, ct);
        await db.SaveChangesAsync(ct);
 
        log.LogInformation("InApp notification created for order {OrderId} confirmation", e.OrderId);
    }
}
 
public sealed class NotifyOnOrderCancelled(RetailStoreDbContext db, ILogger<NotifyOnOrderCancelled> log)
    : INotificationHandler<OrderCancelledEvent>
{
    public async Task Handle(OrderCancelledEvent e, CancellationToken ct)
    {
        var template = await db.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Name == "OrderCancelled"
                && t.Channel == NotificationChannel.InApp && t.IsActive, ct);
 
        if (template is null) return;
 
        var variables = new Dictionary<string, string>
        {
            ["OrderId"] = e.OrderId.ToString()[..8],
            ["Reason"] = e.Reason
        };
 
        var notification = Notification.CreateFromTemplate(
            template, variables, e.CustomerId, RecipientType.Customer,
            referenceType: "Order", referenceId: e.OrderId);
 
        await db.Set<Notification>().AddAsync(notification, ct);
        await db.SaveChangesAsync(ct);
    }
}
 
// ─── Shipping Events → Notifications ────────────────────────
public sealed class NotifyOnShipmentShipped(RetailStoreDbContext db, ILogger<NotifyOnShipmentShipped> log)
    : INotificationHandler<ShipmentShippedEvent>
{
    public async Task Handle(ShipmentShippedEvent e, CancellationToken ct)
    {
        var template = await db.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Name == "ShipmentShipped"
                && t.Channel == NotificationChannel.InApp && t.IsActive, ct);
 
        if (template is null) return;
 
        var variables = new Dictionary<string, string>
        {
            ["OrderId"] = e.OrderId.ToString()[..8],
            ["Carrier"] = e.Carrier,
            ["TrackingNumber"] = e.TrackingNumber
        };
 
        var notification = Notification.CreateFromTemplate(
            template, variables, e.CustomerId, RecipientType.Customer,
            priority: NotificationPriority.High,
            referenceType: "Shipment", referenceId: e.ShipmentId);
 
        await db.Set<Notification>().AddAsync(notification, ct);
        await db.SaveChangesAsync(ct);
    }
}
 
public sealed class NotifyOnShipmentDelivered(RetailStoreDbContext db, ILogger<NotifyOnShipmentDelivered> log)
    : INotificationHandler<ShipmentDeliveredEvent>
{
    public async Task Handle(ShipmentDeliveredEvent e, CancellationToken ct)
    {
        var template = await db.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Name == "ShipmentDelivered"
                && t.Channel == NotificationChannel.InApp && t.IsActive, ct);
 
        if (template is null) return;
 
        var variables = new Dictionary<string, string>
        {
            ["OrderId"] = e.OrderId.ToString()[..8]
        };
 
        var notification = Notification.CreateFromTemplate(
            template, variables, e.CustomerId, RecipientType.Customer,
            referenceType: "Shipment", referenceId: e.ShipmentId);
 
        await db.Set<Notification>().AddAsync(notification, ct);
        await db.SaveChangesAsync(ct);
    }
}
 
// ─── Inventory Events → Notifications ───────────────────────
public sealed class NotifyOnLowStock(RetailStoreDbContext db, ILogger<NotifyOnLowStock> log)
    : INotificationHandler<LowStockAlertEvent>
{
    public async Task Handle(LowStockAlertEvent e, CancellationToken ct)
    {
        var template = await db.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Name == "LowStockAlert"
                && t.Channel == NotificationChannel.InApp && t.IsActive, ct);
 
        if (template is null) return;
 
        // Get product info
        var product = await db.Set<Products.Domain.Product>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == e.ProductId, ct);
 
        var variables = new Dictionary<string, string>
        {
            ["ProductName"] = product?.Name ?? "Unknown",
            ["Sku"] = product?.Sku ?? "N/A",
            ["CurrentQuantity"] = e.CurrentQuantity.ToString(),
            ["Threshold"] = e.Threshold.ToString()
        };
 
        // Send to all admin/manager users as InApp notifications
        var adminUsers = await db.Set<Users.Domain.User>()
            .AsNoTracking()
            .Where(u => u.IsActive)
            .ToListAsync(ct);
 
        foreach (var user in adminUsers)
        {
            var notification = Notification.CreateFromTemplate(
                template, variables, user.Id, RecipientType.User,
                priority: NotificationPriority.High,
                referenceType: "InventoryItem", referenceId: e.InventoryItemId);
 
            await db.Set<Notification>().AddAsync(notification, ct);
        }
 
        await db.SaveChangesAsync(ct);
        log.LogInformation("Low stock notifications sent to {Count} users for product {ProductId}",
            adminUsers.Count, e.ProductId);
    }
}
 
// ─── User Events → Notifications ────────────────────────────
public sealed class NotifyOnUserRegistered(RetailStoreDbContext db, ILogger<NotifyOnUserRegistered> log)
    : INotificationHandler<UserRegisteredEvent>
{
    public async Task Handle(UserRegisteredEvent e, CancellationToken ct)
    {
        var template = await db.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Name == "WelcomeUser"
                && t.Channel == NotificationChannel.InApp && t.IsActive, ct);
 
        if (template is null) return;
 
        var variables = new Dictionary<string, string>
        {
            ["Username"] = e.Username,
            ["Email"] = e.Email
        };
 
        var notification = Notification.CreateFromTemplate(
            template, variables, e.UserId, RecipientType.User,
            recipientEmail: e.Email);
 
        await db.Set<Notification>().AddAsync(notification, ct);
        await db.SaveChangesAsync(ct);
    }
}