using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailStore.Api.Features.Providers.Application.Commands;
using RetailStore.Api.Features.Providers.Application.Queries;
 
namespace RetailStore.Api.Features.Providers.Api;
 
[ApiController, Route("api/v1/providers"), Authorize]
public sealed class ProvidersController(ISender sender) : ControllerBase
{
    // ─── Create ─────────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        RegisterProviderCommand cmd, CancellationToken ct)
    {
        var id = await sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
 
    // ─── Update ─────────────────────────────────────────────
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id, UpdateProviderCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { ProviderId = id }, ct); return NoContent(); }
 
    [HttpPut("{id:guid}/email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeEmail(
        Guid id, ChangeProviderEmailCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { ProviderId = id }, ct); return NoContent(); }
 
    // ─── Product Association ────────────────────────────────
    [HttpPost("{id:guid}/products/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssociateProduct(
        Guid id, Guid productId, CancellationToken ct)
    { await sender.Send(new AssociateProductCommand(id, productId), ct); return NoContent(); }
 
    [HttpDelete("{id:guid}/products/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DissociateProduct(
        Guid id, Guid productId, CancellationToken ct)
    { await sender.Send(new DissociateProductCommand(id, productId), ct); return NoContent(); }
 
    // ─── Lifecycle ──────────────────────────────────────────
    [HttpPut("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    { await sender.Send(new DeactivateProviderCommand(id), ct); return NoContent(); }
 
    [HttpPut("{id:guid}/reactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    { await sender.Send(new ReactivateProviderCommand(id), ct); return NoContent(); }
 
    // ─── Queries ────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search, [FromQuery] bool? isActive, CancellationToken ct)
        => Ok(await sender.Send(new GetProvidersQuery(search, isActive), ct));
 
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetProviderByIdQuery(id), ct));
 
    [HttpGet("by-product/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProduct(Guid productId, CancellationToken ct)
        => Ok(await sender.Send(new GetProvidersByProductQuery(productId), ct));
}