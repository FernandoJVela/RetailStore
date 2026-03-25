using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailStore.Api.Features.Customers.Application.Commands;
using RetailStore.Api.Features.Customers.Application.Queries;
 
namespace RetailStore.Api.Features.Customers.Api;
 
[ApiController]
[Route("api/v1/customers")]
[Authorize]
public sealed class CustomersController(ISender sender) : ControllerBase
{
    // ─── Create ─────────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        RegisterCustomerCommand command, CancellationToken ct)
    {
        var id = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
 
    // ─── Update Profile ─────────────────────────────────────
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id, UpdateCustomerCommand command, CancellationToken ct)
    {
        await sender.Send(command with { CustomerId = id }, ct);
        return NoContent();
    }
 
    // ─── Change Email ───────────────────────────────────────
    [HttpPut("{id:guid}/email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeEmail(
        Guid id, ChangeCustomerEmailCommand command, CancellationToken ct)
    {
        await sender.Send(command with { CustomerId = id }, ct);
        return NoContent();
    }
 
    // ─── Update Shipping Address ────────────────────────────
    [HttpPut("{id:guid}/address")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAddress(
        Guid id, UpdateShippingAddressCommand command, CancellationToken ct)
    {
        await sender.Send(command with { CustomerId = id }, ct);
        return NoContent();
    }
 
    // ─── Deactivate ─────────────────────────────────────────
    [HttpPut("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeactivateCustomerCommand(id), ct);
        return NoContent();
    }
 
    // ─── Reactivate ─────────────────────────────────────────
    [HttpPut("{id:guid}/reactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        await sender.Send(new ReactivateCustomerCommand(id), ct);
        return NoContent();
    }
 
    // ─── Queries ────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        CancellationToken ct)
        => Ok(await sender.Send(new GetCustomersQuery(search, isActive), ct));
 
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetCustomerByIdQuery(id), ct));
 
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmail(string email, CancellationToken ct)
        => Ok(await sender.Send(new GetCustomerByEmailQuery(email), ct));
}