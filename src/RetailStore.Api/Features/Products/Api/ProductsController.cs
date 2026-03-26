using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailStore.Api.Features.Products.Application.Commands;
using RetailStore.Api.Features.Products.Application.Queries;
 
namespace RetailStore.Api.Features.Products.Api;
 
[ApiController, Route("api/v1/products"), Authorize]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    // ─── Create ─────────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CreateProductCommand command, CancellationToken ct)
    {
        var id = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
 
    // ─── Update Details ─────────────────────────────────────
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDetails(
        Guid id, UpdateProductDetailsCommand command, CancellationToken ct)
    {
        await sender.Send(command with { ProductId = id }, ct);
        return NoContent();
    }
 
    // ─── Update Price ───────────────────────────────────────
    [HttpPut("{id:guid}/price")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdatePrice(
        Guid id, UpdateProductPriceCommand command, CancellationToken ct)
    {
        await sender.Send(command with { ProductId = id }, ct);
        return NoContent();
    }
 
    // ─── Deactivate ─────────────────────────────────────────
    [HttpPut("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeactivateProductCommand(id), ct);
        return NoContent();
    }
 
    // ─── Reactivate ─────────────────────────────────────────
    [HttpPut("{id:guid}/reactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        await sender.Send(new ReactivateProductCommand(id), ct);
        return NoContent();
    }
 
    // ─── Queries ────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        CancellationToken ct)
        => Ok(await sender.Send(new GetProductsQuery(category, isActive, search), ct));
 
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetProductByIdQuery(id), ct));
 
    [HttpGet("category/{category}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(string category, CancellationToken ct)
        => Ok(await sender.Send(new GetProductsByCategoryQuery(category), ct));
}