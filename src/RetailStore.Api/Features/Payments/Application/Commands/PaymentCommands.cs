using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Payments.Application;
using RetailStore.Api.Features.Payments.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.Enums;
 
namespace RetailStore.Api.Features.Payments.Application.Commands;
 
// ═══════════════════════════════════════════════════════════
// CREATE PAYMENT
// ═══════════════════════════════════════════════════════════
public sealed record CreatePaymentCommand(
    Guid OrderId, string Method, string? MethodDetail = null,
    string? GatewayName = null
) : ICommand<Guid>, IRequirePermission
{
    public string RequiredPermission => "payments:write";
}
 
public sealed class CreatePaymentValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Method).NotEmpty()
            .Must(m => Enum.TryParse<PaymentMethod>(m, true, out _))
            .WithMessage("Invalid method. Use: CreditCard, DebitCard, BankTransfer, Cash, DigitalWallet, PSE");
    }
}
 
public sealed class CreatePaymentHandler(
    IPaymentRepository payments, RetailStoreDbContext db)
    : IRequestHandler<CreatePaymentCommand, Guid>
{
    public async Task<Guid> Handle(CreatePaymentCommand cmd, CancellationToken ct)
    {
        if (await payments.ExistsForOrderAsync(cmd.OrderId, ct))
            throw new DomainException(PaymentErrors.AlreadyExists(cmd.OrderId));
 
        var order = await db.Set<Order>()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct)
            ?? throw new DomainException(OrderErrors.OrderNotFound(cmd.OrderId));
 
        if (order.Status != OrderStatus.Confirmed)
            throw new DomainException(PaymentErrors.OrderNotConfirmed());
 
        var method = Enum.Parse<PaymentMethod>(cmd.Method, true);
        var total = order.Items.Sum(i => i.UnitPrice * i.Quantity);
 
        var payment = Payment.Create(
            order.Id, order.CustomerId, total, "USD", method, cmd.MethodDetail);
 
        if (!string.IsNullOrEmpty(cmd.GatewayName))
            payment.SetGateway(cmd.GatewayName, string.Empty);
 
        await payments.AddAsync(payment, ct);
        return payment.Id;
    }
}
 
// ═══════════════════════════════════════════════════════════
// AUTHORIZE PAYMENT
// ═══════════════════════════════════════════════════════════
public sealed record AuthorizePaymentCommand(
    Guid PaymentId, string? GatewayTransactionId = null, string? GatewayResponse = null
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "payments:write";
}
 
public sealed class AuthorizePaymentHandler(IPaymentRepository payments)
    : IRequestHandler<AuthorizePaymentCommand, Unit>
{
    public async Task<Unit> Handle(AuthorizePaymentCommand cmd, CancellationToken ct)
    {
        var payment = await payments.GetByIdAsync(cmd.PaymentId, ct)
            ?? throw new DomainException(PaymentErrors.NotFound(cmd.PaymentId));
 
        if (!string.IsNullOrEmpty(cmd.GatewayTransactionId))
            payment.SetGateway(payment.GatewayName ?? "Manual",
                cmd.GatewayTransactionId, cmd.GatewayResponse);
 
        payment.Authorize();
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// CAPTURE PAYMENT
// ═══════════════════════════════════════════════════════════
public sealed record CapturePaymentCommand(Guid PaymentId) : ICommand, IRequirePermission
{ public string RequiredPermission => "payments:write"; }
 
public sealed class CapturePaymentHandler(IPaymentRepository payments)
    : IRequestHandler<CapturePaymentCommand, Unit>
{
    public async Task<Unit> Handle(CapturePaymentCommand cmd, CancellationToken ct)
    {
        var payment = await payments.GetByIdAsync(cmd.PaymentId, ct)
            ?? throw new DomainException(PaymentErrors.NotFound(cmd.PaymentId));
        payment.Capture();
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// FAIL PAYMENT
// ═══════════════════════════════════════════════════════════
public sealed record FailPaymentCommand(Guid PaymentId, string Reason) : ICommand, IRequirePermission
{ public string RequiredPermission => "payments:write"; }
 
public sealed class FailPaymentHandler(IPaymentRepository payments)
    : IRequestHandler<FailPaymentCommand, Unit>
{
    public async Task<Unit> Handle(FailPaymentCommand cmd, CancellationToken ct)
    {
        var payment = await payments.GetByIdAsync(cmd.PaymentId, ct)
            ?? throw new DomainException(PaymentErrors.NotFound(cmd.PaymentId));
        payment.Fail(cmd.Reason);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// CANCEL PAYMENT
// ═══════════════════════════════════════════════════════════
public sealed record CancelPaymentCommand(Guid PaymentId, string? Reason = null) : ICommand, IRequirePermission
{ public string RequiredPermission => "payments:write"; }
 
public sealed class CancelPaymentHandler(IPaymentRepository payments)
    : IRequestHandler<CancelPaymentCommand, Unit>
{
    public async Task<Unit> Handle(CancelPaymentCommand cmd, CancellationToken ct)
    {
        var payment = await payments.GetByIdAsync(cmd.PaymentId, ct)
            ?? throw new DomainException(PaymentErrors.NotFound(cmd.PaymentId));
        payment.Cancel(cmd.Reason);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// REQUEST REFUND
// ═══════════════════════════════════════════════════════════
public sealed record RequestRefundCommand(
    Guid PaymentId, decimal Amount, string Reason
) : ICommand<Guid>, IRequirePermission
{
    public string RequiredPermission => "payments:refund";
}
 
public sealed class RequestRefundValidator : AbstractValidator<RequestRefundCommand>
{
    public RequestRefundValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
 
public sealed class RequestRefundHandler(IPaymentRepository payments)
    : IRequestHandler<RequestRefundCommand, Guid>
{
    public async Task<Guid> Handle(RequestRefundCommand cmd, CancellationToken ct)
    {
        var payment = await payments.GetByIdAsync(cmd.PaymentId, ct)
            ?? throw new DomainException(PaymentErrors.NotFound(cmd.PaymentId));
        var refund = payment.RequestRefund(cmd.Amount, cmd.Reason);
        return refund.Id;
    }
}
 
// ═══════════════════════════════════════════════════════════
// COMPLETE REFUND
// ═══════════════════════════════════════════════════════════
public sealed record CompleteRefundCommand(Guid PaymentId, Guid RefundId) : ICommand, IRequirePermission
{ public string RequiredPermission => "payments:refund"; }
 
public sealed class CompleteRefundHandler(IPaymentRepository payments)
    : IRequestHandler<CompleteRefundCommand, Unit>
{
    public async Task<Unit> Handle(CompleteRefundCommand cmd, CancellationToken ct)
    {
        var payment = await payments.GetByIdAsync(cmd.PaymentId, ct)
            ?? throw new DomainException(PaymentErrors.NotFound(cmd.PaymentId));
        payment.CompleteRefund(cmd.RefundId);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// FAIL REFUND
// ═══════════════════════════════════════════════════════════
public sealed record FailRefundCommand(Guid PaymentId, Guid RefundId, string Reason) : ICommand, IRequirePermission
{ public string RequiredPermission => "payments:refund"; }
 
public sealed class FailRefundHandler(IPaymentRepository payments)
    : IRequestHandler<FailRefundCommand, Unit>
{
    public async Task<Unit> Handle(FailRefundCommand cmd, CancellationToken ct)
    {
        var payment = await payments.GetByIdAsync(cmd.PaymentId, ct)
            ?? throw new DomainException(PaymentErrors.NotFound(cmd.PaymentId));
        payment.FailRefund(cmd.RefundId, cmd.Reason);
        return Unit.Value;
    }
}