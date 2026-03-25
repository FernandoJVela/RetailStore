using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailStore.Api.Features.Orders.Application.Commands;
using RetailStore.Api.Features.Orders.Application.Queries;

namespace RetailStore.Api.Features.Orders.Api;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
        => Ok(await _sender.Send(new GetOrdersQuery()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await _sender.Send(new GetOrderByIdQuery(id)));

    [HttpGet("item/{id:guid}")]
    public async Task<IActionResult> GetOrderItemById(Guid id)
        => Ok(await _sender.Send(new GetOrderItemByIdQuery(id)));
}