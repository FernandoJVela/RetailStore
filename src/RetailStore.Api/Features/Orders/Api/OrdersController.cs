using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailStore.Api.Features.Orders.Application.Commands;
using RetailStore.Api.Features.Orders.Application.Queries;
 
namespace RetailStore.Api.Features.Orders.Api;
 
[ApiController, Route("api/v1/orders"), Authorize]
public sealed class OrdersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateOrderCommand cmd, CancellationToken ct)
        => CreatedAtAction(nameof(GetById), new { id = await sender.Send(cmd, ct) }, null);
 
    [HttpPost("{orderId:guid}/items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddItem(Guid orderId, AddOrderItemCommand cmd, CancellationToken ct)
    { 
        await sender.Send(cmd with { OrderId = orderId }, ct); 
        return NoContent(); 
    }
 
    [HttpDelete("{orderId:guid}/items/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveItem(Guid orderId, Guid productId, CancellationToken ct)
    { 
        await sender.Send(new RemoveOrderItemCommand(orderId, productId), ct); 
        return NoContent(); 
    }
 
    [HttpPut("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    { 
        await sender.Send(new ConfirmOrderCommand(id), ct); 
        return NoContent(); 
    }
 
    [HttpPut("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    { 
        await sender.Send(new CompleteOrderCommand(id), ct); 
        return NoContent(); 
    }
 
    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(Guid id, CancelOrderCommand cmd, CancellationToken ct)
    { 
        await sender.Send(cmd with { OrderId = id }, ct); 
        return NoContent(); 
    }
 
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken ct)
        => Ok(await sender.Send(new GetOrdersQuery(status), ct));
 
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetOrderByIdQuery(id), ct));
}