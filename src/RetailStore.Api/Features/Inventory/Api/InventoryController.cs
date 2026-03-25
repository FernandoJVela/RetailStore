using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailStore.Api.Features.Inventory.Application.Commands;
using RetailStore.Api.Features.Inventory.Application.Queries;
 
namespace RetailStore.Api.Features.Inventory.Api;
 
[ApiController, Route("api/v1/inventory"), Authorize]
public sealed class InventoryController(ISender sender) : ControllerBase
{
    // ─── Create inventory record for a product ──────────────
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CreateInventoryItemCommand cmd, CancellationToken ct)
    {
        var id = await sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetByProduct),
            new { productId = cmd.ProductId }, id);
    }
 
    // ─── Stock Operations ───────────────────────────────────
    [HttpPut("{productId:guid}/add-stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddStock(
        Guid productId, AddStockCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { ProductId = productId }, ct); return NoContent(); }
 
    [HttpPut("{productId:guid}/remove-stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RemoveStock(
        Guid productId, RemoveStockCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { ProductId = productId }, ct); return NoContent(); }
 
    [HttpPut("{productId:guid}/reserve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Reserve(
        Guid productId, ReserveStockCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { ProductId = productId }, ct); return NoContent(); }
 
    [HttpPut("{productId:guid}/release")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Release(
        Guid productId, ReleaseReservationCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { ProductId = productId }, ct); return NoContent(); }
 
    [HttpPut("{productId:guid}/fulfill")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Fulfill(
        Guid productId, FulfillReservationCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { ProductId = productId }, ct); return NoContent(); }
 
    // ─── Adjustments ────────────────────────────────────────
    [HttpPut("{productId:guid}/adjust")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Adjust(
        Guid productId, AdjustStockCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { ProductId = productId }, ct); return NoContent(); }
 
    [HttpPut("{productId:guid}/reorder-threshold")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateThreshold(
        Guid productId, UpdateReorderThresholdCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { ProductId = productId }, ct); return NoContent(); }
 
    // ─── Queries ────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? stockStatus, CancellationToken ct)
        => Ok(await sender.Send(new GetInventoryQuery(stockStatus), ct));
 
    [HttpGet("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProduct(
        Guid productId, CancellationToken ct)
        => Ok(await sender.Send(new GetInventoryByProductQuery(productId), ct));
 
    [HttpGet("low-stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStock(CancellationToken ct)
        => Ok(await sender.Send(new GetLowStockQuery(), ct));
}