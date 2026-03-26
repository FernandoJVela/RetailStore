using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailStore.Api.Features.Payments.Application.Commands;
using RetailStore.Api.Features.Payments.Application.Queries;
 
namespace RetailStore.Api.Features.Payments.Api;
 
[ApiController, Route("api/v1/payments"), Authorize]
public sealed class PaymentsController(ISender sender) : ControllerBase
{
    // ─── Create ─────────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CreatePaymentCommand cmd, CancellationToken ct)
    {
        var id = await sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
 
    // ─── Payment Lifecycle ──────────────────────────────────
    [HttpPut("{id:guid}/authorize")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Authorize(
        Guid id, AuthorizePaymentCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { PaymentId = id }, ct); return NoContent(); }
 
    [HttpPut("{id:guid}/capture")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Capture(Guid id, CancellationToken ct)
    { await sender.Send(new CapturePaymentCommand(id), ct); return NoContent(); }
 
    [HttpPut("{id:guid}/fail")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Fail(
        Guid id, FailPaymentCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { PaymentId = id }, ct); return NoContent(); }
 
    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(
        Guid id, CancelPaymentCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { PaymentId = id }, ct); return NoContent(); }
 
    // ─── Refunds ────────────────────────────────────────────
    [HttpPost("{id:guid}/refunds")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> RequestRefund(
        Guid id, RequestRefundCommand cmd, CancellationToken ct)
    {
        var refundId = await sender.Send(cmd with { PaymentId = id }, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { refundId });
    }
 
    [HttpPut("{id:guid}/refunds/{refundId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CompleteRefund(
        Guid id, Guid refundId, CancellationToken ct)
    { await sender.Send(new CompleteRefundCommand(id, refundId), ct); return NoContent(); }
 
    [HttpPut("{id:guid}/refunds/{refundId:guid}/fail")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> FailRefund(
        Guid id, Guid refundId, FailRefundCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { PaymentId = id, RefundId = refundId }, ct); return NoContent(); }
 
    // ─── Queries ────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status, [FromQuery] Guid? customerId, CancellationToken ct)
        => Ok(await sender.Send(new GetPaymentsQuery(status, customerId), ct));
 
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetPaymentByIdQuery(id), ct));
 
    [HttpGet("by-order/{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrder(Guid orderId, CancellationToken ct)
        => Ok(await sender.Send(new GetPaymentByOrderQuery(orderId), ct));
}