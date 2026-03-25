using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Customers.Domain;
 
public sealed record CustomerRegisteredEvent(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string Email) : DomainEvent
{
    public override string EventType => "CustomerRegistered";
}
 
public sealed record CustomerUpdatedEvent(
    Guid CustomerId,
    string FirstName,
    string LastName) : DomainEvent
{
    public override string EventType => "CustomerUpdated";
}
 
public sealed record CustomerEmailChangedEvent(
    Guid CustomerId,
    string OldEmail,
    string NewEmail) : DomainEvent
{
    public override string EventType => "CustomerEmailChanged";
}
 
public sealed record CustomerAddressUpdatedEvent(
    Guid CustomerId,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country) : DomainEvent
{
    public override string EventType => "CustomerAddressUpdated";
}
 
public sealed record CustomerDeactivatedEvent(
    Guid CustomerId,
    string Email) : DomainEvent
{
    public override string EventType => "CustomerDeactivated";
}
 
public sealed record CustomerReactivatedEvent(
    Guid CustomerId,
    string Email) : DomainEvent
{
    public override string EventType => "CustomerReactivated";
}