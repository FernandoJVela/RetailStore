using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Customers.Application;
using RetailStore.Api.Features.Customers.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Customers.Application.Commands;
 
// ═══════════════════════════════════════════════════════════
// UPDATE CUSTOMER NAME / PHONE
// ═══════════════════════════════════════════════════════════
public sealed record UpdateCustomerCommand(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string? Phone = null
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "customers:write";
}
 
public sealed class UpdateCustomerValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.Phone).MaximumLength(20).When(x => x.Phone is not null);
    }
}
 
public sealed class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Unit>
{
    private readonly ICustomerRepository _customers;
    public UpdateCustomerHandler(ICustomerRepository customers) => _customers = customers;
 
    public async Task<Unit> Handle(UpdateCustomerCommand cmd, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(cmd.CustomerId, ct)
            ?? throw new DomainException(CustomerErrors.NotFound(cmd.CustomerId));
 
        customer.UpdateName(cmd.FirstName, cmd.LastName);
        customer.UpdatePhone(cmd.Phone);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// CHANGE EMAIL
// ═══════════════════════════════════════════════════════════
public sealed record ChangeCustomerEmailCommand(
    Guid CustomerId,
    string NewEmail
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "customers:write";
}
 
public sealed class ChangeCustomerEmailValidator : AbstractValidator<ChangeCustomerEmailCommand>
{
    public ChangeCustomerEmailValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.NewEmail).NotEmpty().EmailAddress().MaximumLength(256);
    }
}
 
public sealed class ChangeCustomerEmailHandler : IRequestHandler<ChangeCustomerEmailCommand, Unit>
{
    private readonly ICustomerRepository _customers;
    public ChangeCustomerEmailHandler(ICustomerRepository customers) => _customers = customers;
 
    public async Task<Unit> Handle(ChangeCustomerEmailCommand cmd, CancellationToken ct)
    {
        // Check if new email is already taken by another customer
        var existing = await _customers.GetByEmailAsync(cmd.NewEmail, ct);
        if (existing is not null && existing.Id != cmd.CustomerId)
            throw new DomainException(CustomerErrors.DuplicateEmail(cmd.NewEmail));
 
        var customer = await _customers.GetByIdAsync(cmd.CustomerId, ct)
            ?? throw new DomainException(CustomerErrors.NotFound(cmd.CustomerId));
 
        customer.ChangeEmail(cmd.NewEmail);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// UPDATE SHIPPING ADDRESS
// ═══════════════════════════════════════════════════════════
public sealed record UpdateShippingAddressCommand(
    Guid CustomerId,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "customers:write";
}
 
public sealed class UpdateShippingAddressValidator : AbstractValidator<UpdateShippingAddressCommand>
{
    public UpdateShippingAddressValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Street).NotEmpty().MaximumLength(300);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).MaximumLength(100);
        RuleFor(x => x.ZipCode).MaximumLength(20);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}
 
public sealed class UpdateShippingAddressHandler : IRequestHandler<UpdateShippingAddressCommand, Unit>
{
    private readonly ICustomerRepository _customers;
    public UpdateShippingAddressHandler(ICustomerRepository customers) => _customers = customers;
 
    public async Task<Unit> Handle(UpdateShippingAddressCommand cmd, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(cmd.CustomerId, ct)
            ?? throw new DomainException(CustomerErrors.NotFound(cmd.CustomerId));
 
        var address = new Address(cmd.Street, cmd.City, cmd.State, cmd.ZipCode, cmd.Country);
        customer.UpdateShippingAddress(address);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// DEACTIVATE CUSTOMER
// ═══════════════════════════════════════════════════════════
public sealed record DeactivateCustomerCommand(Guid CustomerId) : ICommand, IRequirePermission
{
    public string RequiredPermission => "customers:write";
}
 
public sealed class DeactivateCustomerHandler : IRequestHandler<DeactivateCustomerCommand, Unit>
{
    private readonly ICustomerRepository _customers;
    public DeactivateCustomerHandler(ICustomerRepository customers) => _customers = customers;
 
    public async Task<Unit> Handle(DeactivateCustomerCommand cmd, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(cmd.CustomerId, ct)
            ?? throw new DomainException(CustomerErrors.NotFound(cmd.CustomerId));
 
        customer.Deactivate();
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// REACTIVATE CUSTOMER
// ═══════════════════════════════════════════════════════════
public sealed record ReactivateCustomerCommand(Guid CustomerId) : ICommand, IRequirePermission
{
    public string RequiredPermission => "customers:write";
}
 
public sealed class ReactivateCustomerHandler : IRequestHandler<ReactivateCustomerCommand, Unit>
{
    private readonly ICustomerRepository _customers;
    public ReactivateCustomerHandler(ICustomerRepository customers) => _customers = customers;
 
    public async Task<Unit> Handle(ReactivateCustomerCommand cmd, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(cmd.CustomerId, ct)
            ?? throw new DomainException(CustomerErrors.NotFound(cmd.CustomerId));
 
        customer.Reactivate();
        return Unit.Value;
    }
}