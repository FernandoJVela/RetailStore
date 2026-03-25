using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Customers.Application;
using RetailStore.Api.Features.Customers.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Customers.Application.Commands;
 
// ─── Command ───────────────────────────────────────────────
public sealed record RegisterCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Phone = null,
    ShippingAddressDto? ShippingAddress = null
) : ICommand<Guid>, IRequirePermission
{
    public string RequiredPermission => "customers:write";
}
 
public sealed record ShippingAddressDto(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country);
 
// ─── Validator ─────────────────────────────────────────────
public sealed class RegisterCustomerValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters.")
            .MaximumLength(100);
 
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters.")
            .MaximumLength(100);
 
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256);
 
        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => x.Phone is not null);
 
        When(x => x.ShippingAddress is not null, () =>
        {
            RuleFor(x => x.ShippingAddress!.Street)
                .NotEmpty().WithMessage("Street is required.")
                .MaximumLength(300);
            RuleFor(x => x.ShippingAddress!.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(100);
            RuleFor(x => x.ShippingAddress!.Country)
                .NotEmpty().WithMessage("Country is required.")
                .MaximumLength(100);
        });
    }
}
 
// ─── Handler ──────────────────────────────────────────────
public sealed class RegisterCustomerHandler : IRequestHandler<RegisterCustomerCommand, Guid>
{
    private readonly ICustomerRepository _customers;
 
    public RegisterCustomerHandler(ICustomerRepository customers)
        => _customers = customers;
 
    public async Task<Guid> Handle(RegisterCustomerCommand cmd, CancellationToken ct)
    {
        // Check email uniqueness
        if (await _customers.ExistsWithEmailAsync(cmd.Email, ct))
            throw new DomainException(CustomerErrors.DuplicateEmail(cmd.Email));
 
        var customer = Customer.Register(
            cmd.FirstName, cmd.LastName, cmd.Email, cmd.Phone);
 
        // Set shipping address if provided
        if (cmd.ShippingAddress is not null)
        {
            var address = new Address(
                cmd.ShippingAddress.Street,
                cmd.ShippingAddress.City,
                cmd.ShippingAddress.State,
                cmd.ShippingAddress.ZipCode,
                cmd.ShippingAddress.Country);
 
            customer.UpdateShippingAddress(address);
        }
 
        await _customers.AddAsync(customer, ct);
        return customer.Id;
    }
}