using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailStore.Api.Features.Shipping.Application.Commands;
using RetailStore.Api.Features.Shipping.Application.Queries;
 
namespace RetailStore.Api.Features.Shipping.Api;
 
[ApiController, Route("api/v1/shipments"), Authorize]
public sealed class ShipmentsController(ISender sender) : ControllerBase
{
    // ─── Create shipment from order ─────────────────────────
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CreateShipmentCommand cmd, CancellationToken ct)
    {
        var id = await sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
 
    // ─── Carrier & Cost ─────────────────────────────────────
    [HttpPut("{id:guid}/carrier")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignCarrier(
        Guid id, AssignCarrierCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { ShipmentId = id }, ct); return NoContent(); }
 
    [HttpPut("{id:guid}/cost")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetCost(
        Guid id, SetShippingCostCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { ShipmentId = id }, ct); return NoContent(); }
 
    // ─── Status Transitions ─────────────────────────────────
    [HttpPut("{id:guid}/ship")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkShipped(Guid id, CancellationToken ct)
    { await sender.Send(new MarkShippedCommand(id), ct); return NoContent(); }
 
    [HttpPut("{id:guid}/in-transit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkInTransit(Guid id, CancellationToken ct)
    { await sender.Send(new MarkInTransitCommand(id), ct); return NoContent(); }
 
    [HttpPut("{id:guid}/deliver")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkDelivered(Guid id, CancellationToken ct)
    { await sender.Send(new MarkDeliveredCommand(id), ct); return NoContent(); }
 
    [HttpPut("{id:guid}/fail")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkFailed(
        Guid id, MarkFailedCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { ShipmentId = id }, ct); return NoContent(); }
 
    [HttpPut("{id:guid}/return")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkReturned(
        Guid id, MarkReturnedCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { ShipmentId = id }, ct); return NoContent(); }
 
    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(
        Guid id, CancelShipmentCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { ShipmentId = id }, ct); return NoContent(); }
 
    // ─── Queries ────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] Guid? customerId,
        CancellationToken ct)
        => Ok(await sender.Send(new GetShipmentsQuery(status, customerId), ct));
 
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetShipmentByIdQuery(id), ct));
 
    [HttpGet("by-order/{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrder(Guid orderId, CancellationToken ct)
        => Ok(await sender.Send(new GetShipmentByOrderQuery(orderId), ct));
 
    [HttpGet("track/{trackingNumber}")]
    [AllowAnonymous]  // Public tracking endpoint
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Track(string trackingNumber, CancellationToken ct)
        => Ok(await sender.Send(new TrackShipmentQuery(trackingNumber), ct));
}