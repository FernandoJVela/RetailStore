using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.Api.Features.Providers.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Providers.Application.Commands;
 
// ═══════════════════════════════════════════════════════════
// REGISTER PROVIDER
// ═══════════════════════════════════════════════════════════
public sealed record RegisterProviderCommand(
    string CompanyName, string ContactName,
    string Email, string? Phone = null
) : ICommand<Guid>, IRequirePermission
{
    public string RequiredPermission => "providers:write";
}
 
public sealed class RegisterProviderValidator : AbstractValidator<RegisterProviderCommand>
{
    public RegisterProviderValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MinimumLength(2).MaximumLength(200);
        RuleFor(x => x.ContactName).NotEmpty().MinimumLength(2).MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(20).When(x => x.Phone is not null);
    }
}
 
public sealed class RegisterProviderHandler(IProviderRepository providers)
    : IRequestHandler<RegisterProviderCommand, Guid>
{
    public async Task<Guid> Handle(RegisterProviderCommand cmd, CancellationToken ct)
    {
        if (await providers.ExistsWithEmailAsync(cmd.Email, ct))
            throw new DomainException(ProviderErrors.DuplicateEmail(cmd.Email));
 
        var provider = Provider.Register(
            cmd.CompanyName, cmd.ContactName, cmd.Email, cmd.Phone);
 
        await providers.AddAsync(provider, ct);
        return provider.Id;
    }
}
 
// ═══════════════════════════════════════════════════════════
// UPDATE PROVIDER
// ═══════════════════════════════════════════════════════════
public sealed record UpdateProviderCommand(
    Guid ProviderId, string CompanyName,
    string ContactName, string? Phone = null
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "providers:write";
}
 
public sealed class UpdateProviderValidator : AbstractValidator<UpdateProviderCommand>
{
    public UpdateProviderValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.CompanyName).NotEmpty().MinimumLength(2).MaximumLength(200);
        RuleFor(x => x.ContactName).NotEmpty().MinimumLength(2).MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(20).When(x => x.Phone is not null);
    }
}
 
public sealed class UpdateProviderHandler(IProviderRepository providers)
    : IRequestHandler<UpdateProviderCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProviderCommand cmd, CancellationToken ct)
    {
        var provider = await providers.GetByIdAsync(cmd.ProviderId, ct)
            ?? throw new DomainException(ProviderErrors.NotFound(cmd.ProviderId));
 
        provider.UpdateContactInfo(cmd.CompanyName, cmd.ContactName, cmd.Phone);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// CHANGE EMAIL
// ═══════════════════════════════════════════════════════════
public sealed record ChangeProviderEmailCommand(
    Guid ProviderId, string NewEmail
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "providers:write";
}
 
public sealed class ChangeProviderEmailValidator : AbstractValidator<ChangeProviderEmailCommand>
{
    public ChangeProviderEmailValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.NewEmail).NotEmpty().EmailAddress().MaximumLength(256);
    }
}
 
public sealed class ChangeProviderEmailHandler(IProviderRepository providers)
    : IRequestHandler<ChangeProviderEmailCommand, Unit>
{
    public async Task<Unit> Handle(ChangeProviderEmailCommand cmd, CancellationToken ct)
    {
        var existing = await providers.GetByEmailAsync(cmd.NewEmail, ct);
        if (existing is not null && existing.Id != cmd.ProviderId)
            throw new DomainException(ProviderErrors.DuplicateEmail(cmd.NewEmail));
 
        var provider = await providers.GetByIdAsync(cmd.ProviderId, ct)
            ?? throw new DomainException(ProviderErrors.NotFound(cmd.ProviderId));
 
        provider.ChangeEmail(cmd.NewEmail);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// ASSOCIATE PRODUCT
// ═══════════════════════════════════════════════════════════
public sealed record AssociateProductCommand(
    Guid ProviderId, Guid ProductId
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "providers:write";
}
 
public sealed class AssociateProductHandler(
    IProviderRepository providers,
    IRepository<Product> products)
    : IRequestHandler<AssociateProductCommand, Unit>
{
    public async Task<Unit> Handle(AssociateProductCommand cmd, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(ProductErrors.NotFound(cmd.ProductId));
 
        var provider = await providers.GetByIdAsync(cmd.ProviderId, ct)
            ?? throw new DomainException(ProviderErrors.NotFound(cmd.ProviderId));
 
        if (!provider.IsActive)
            throw new DomainException(ProviderErrors.InactiveProviderCannotSupply());
 
        provider.AssociateProduct(cmd.ProductId);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// DISSOCIATE PRODUCT
// ═══════════════════════════════════════════════════════════
public sealed record DissociateProductCommand(
    Guid ProviderId, Guid ProductId
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "providers:write";
}
 
public sealed class DissociateProductHandler(IProviderRepository providers)
    : IRequestHandler<DissociateProductCommand, Unit>
{
    public async Task<Unit> Handle(DissociateProductCommand cmd, CancellationToken ct)
    {
        var provider = await providers.GetByIdAsync(cmd.ProviderId, ct)
            ?? throw new DomainException(ProviderErrors.NotFound(cmd.ProviderId));
 
        provider.DissociateProduct(cmd.ProductId);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// DEACTIVATE / REACTIVATE
// ═══════════════════════════════════════════════════════════
public sealed record DeactivateProviderCommand(Guid ProviderId) : ICommand, IRequirePermission
{ public string RequiredPermission => "providers:write"; }
 
public sealed class DeactivateProviderHandler(IProviderRepository providers)
    : IRequestHandler<DeactivateProviderCommand, Unit>
{
    public async Task<Unit> Handle(DeactivateProviderCommand cmd, CancellationToken ct)
    {
        var provider = await providers.GetByIdAsync(cmd.ProviderId, ct)
            ?? throw new DomainException(ProviderErrors.NotFound(cmd.ProviderId));
        provider.Deactivate();
        return Unit.Value;
    }
}
 
public sealed record ReactivateProviderCommand(Guid ProviderId) : ICommand, IRequirePermission
{ public string RequiredPermission => "providers:write"; }
 
public sealed class ReactivateProviderHandler(IProviderRepository providers)
    : IRequestHandler<ReactivateProviderCommand, Unit>
{
    public async Task<Unit> Handle(ReactivateProviderCommand cmd, CancellationToken ct)
    {
        var provider = await providers.GetByIdAsync(cmd.ProviderId, ct)
            ?? throw new DomainException(ProviderErrors.NotFound(cmd.ProviderId));
        provider.Reactivate();
        return Unit.Value;
    }
}