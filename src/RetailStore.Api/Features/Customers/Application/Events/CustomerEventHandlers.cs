using MediatR;
using Microsoft.Extensions.Logging;
using RetailStore.Api.Features.Customers.Domain;
 
namespace RetailStore.Api.Features.Customers.Application.Events;
 
public sealed class CustomerRegisteredEventHandler
    : INotificationHandler<CustomerRegisteredEvent>
{
    private readonly ILogger<CustomerRegisteredEventHandler> _logger;
 
    public CustomerRegisteredEventHandler(ILogger<CustomerRegisteredEventHandler> logger)
        => _logger = logger;
 
    public Task Handle(CustomerRegisteredEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Customer registered: {FirstName} {LastName} ({Email})",
            notification.FirstName, notification.LastName, notification.Email);
 
        // TODO: Send welcome email
        // TODO: Update search index
        // TODO: Sync to CRM
        return Task.CompletedTask;
    }
}
 
public sealed class CustomerDeactivatedEventHandler
    : INotificationHandler<CustomerDeactivatedEvent>
{
    private readonly ILogger<CustomerDeactivatedEventHandler> _logger;
 
    public CustomerDeactivatedEventHandler(ILogger<CustomerDeactivatedEventHandler> logger)
        => _logger = logger;
 
    public Task Handle(CustomerDeactivatedEvent notification, CancellationToken ct)
    {
        _logger.LogWarning(
            "Customer deactivated: {CustomerId} ({Email})",
            notification.CustomerId, notification.Email);
 
        // TODO: Cancel pending orders for this customer
        // TODO: Notify sales team
        return Task.CompletedTask;
    }
}
 
public sealed class CustomerEmailChangedEventHandler
    : INotificationHandler<CustomerEmailChangedEvent>
{
    private readonly ILogger<CustomerEmailChangedEventHandler> _logger;
 
    public CustomerEmailChangedEventHandler(ILogger<CustomerEmailChangedEventHandler> logger)
        => _logger = logger;
 
    public Task Handle(CustomerEmailChangedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Customer {CustomerId} email changed: {OldEmail} → {NewEmail}",
            notification.CustomerId, notification.OldEmail, notification.NewEmail);
 
        // TODO: Send verification to new email
        // TODO: Notify old email of the change
        return Task.CompletedTask;
    }
}