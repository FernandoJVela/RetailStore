using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Providers.Domain;
 
public sealed record ProviderRegisteredEvent(
    Guid ProviderId, string CompanyName, string Email) : DomainEvent
{ public override string EventType => "ProviderRegistered"; }
 
public sealed record ProviderUpdatedEvent(
    Guid ProviderId, string CompanyName, string ContactName) : DomainEvent
{ public override string EventType => "ProviderUpdated"; }
 
public sealed record ProviderEmailChangedEvent(
    Guid ProviderId, string OldEmail, string NewEmail) : DomainEvent
{ public override string EventType => "ProviderEmailChanged"; }
 
public sealed record ProviderDeactivatedEvent(
    Guid ProviderId, string CompanyName) : DomainEvent
{ public override string EventType => "ProviderDeactivated"; }
 
public sealed record ProviderReactivatedEvent(
    Guid ProviderId, string CompanyName) : DomainEvent
{ public override string EventType => "ProviderReactivated"; }
 
public sealed record ProductAssociatedEvent(
    Guid ProviderId, Guid ProductId, string CompanyName) : DomainEvent
{ public override string EventType => "ProductAssociated"; }
 
public sealed record ProductDissociatedEvent(
    Guid ProviderId, Guid ProductId, string CompanyName) : DomainEvent
{ public override string EventType => "ProductDissociated"; }