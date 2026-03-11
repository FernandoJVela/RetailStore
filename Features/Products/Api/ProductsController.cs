using MediatR;
using Microsoft.AspNetCore.Mvc;
using RetailStore.Api.Features.Products.Application.Commands;
using RetailStore.Api.Features.Products.Application.Queries;

namespace RetailStore.Api.Features.Products.Api;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
        => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateProductCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(
                nameof(GetById),
                new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(
            new GetProductByIdQuery(id), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] bool? isActive,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new GetProductsQuery(category, isActive), ct);
        return Ok(result.Value);
    }
}