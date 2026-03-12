using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailStore.Api.Features.Products.Application.Commands;
using RetailStore.Api.Features.Products.Application.Queries;

namespace RetailStore.Api.Features.Products.Api;

[ApiController]
[Route("api/v1/products")]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateProductCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new GetProductByIdQuery(id), ct));

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] bool? isActive,
        CancellationToken ct)
        => Ok(await _sender.Send(
            new GetProductsQuery(category, isActive), ct));

    [HttpPut("{id:guid}/price")]
    public async Task<IActionResult> UpdatePrice(
        Guid id, UpdateProductPriceCommand command,
        CancellationToken ct)
    {
        await _sender.Send(command with { ProductId = id }, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeactivateProductCommand(id), ct);
        return NoContent();
    }
}