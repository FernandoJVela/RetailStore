using FluentValidation;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Application;

namespace RetailStore.Api.Features.Products.Application.Commands;

// Command
public record CreateProductCommand(
    string Name, string Sku, decimal Price,
    string Category, string? Description
) : ICommand<Guid>;

// Validator
public class CreateProductValidator
    : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Category).NotEmpty();
    }
}

// Handler
public class CreateProductHandler
    : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductHandler(
        IRepository<Product> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateProductCommand cmd, CancellationToken ct)
    {
        var product = Product.Create(
            cmd.Name, cmd.Sku, cmd.Price,
            cmd.Category, cmd.Description);

        await _repository.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(product.Id);
    }
}
