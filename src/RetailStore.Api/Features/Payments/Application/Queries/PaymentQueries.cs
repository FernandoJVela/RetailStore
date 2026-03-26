using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Payments.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Payments.Application.Queries;
 
public sealed record PaymentDto(
    Guid Id, Guid OrderId, Guid CustomerId,
    decimal Amount, string Currency, string Method, string? MethodDetail,
    string Status, string? GatewayName, string? GatewayTransactionId,
    decimal TotalRefunded, decimal NetAmount,
    DateTime? CapturedAt, DateTime CreatedAt);
 
public sealed record PaymentDetailDto(
    Guid Id, Guid OrderId, Guid CustomerId,
    decimal Amount, string Currency, string Method, string? MethodDetail,
    string Status, string? GatewayName, string? GatewayTransactionId,
    DateTime? AuthorizedAt, DateTime? CapturedAt, DateTime? FailedAt,
    DateTime? RefundedAt, DateTime? CancelledAt,
    string? FailureReason, string? Notes,
    decimal TotalRefunded, decimal NetAmount,
    List<RefundDto> Refunds, DateTime CreatedAt);
 
public sealed record RefundDto(
    Guid Id, decimal Amount, string Currency,
    string Reason, string Status, DateTime? ProcessedAt);
 
// ─── Get All Payments ───────────────────────────────────────
public sealed record GetPaymentsQuery(
    string? Status = null, Guid? CustomerId = null
) : IQuery<List<PaymentDto>>;
 
public sealed class GetPaymentsHandler(RetailStoreDbContext db)
    : IRequestHandler<GetPaymentsQuery, List<PaymentDto>>
{
    public async Task<List<PaymentDto>> Handle(GetPaymentsQuery q, CancellationToken ct)
    {
        var query = db.Set<Payment>().AsNoTracking().Include(p => p.Refunds).AsQueryable();
 
        if (!string.IsNullOrEmpty(q.Status) && Enum.TryParse<PaymentStatus>(q.Status, true, out var status))
            query = query.Where(p => p.Status == status);
        if (q.CustomerId.HasValue)
            query = query.Where(p => p.CustomerId == q.CustomerId.Value);
 
        var payments = await query.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
 
        return payments.Select(p => new PaymentDto(
            p.Id, p.OrderId, p.CustomerId,
            p.Amount, p.Currency, p.Method.ToString(), p.MethodDetail,
            p.Status.ToString(), p.GatewayName, p.GatewayTransactionId,
            p.TotalRefunded, p.NetAmount,
            p.CapturedAt, p.CreatedAt)).ToList();
    }
}
 
// ─── Get Payment By Id ──────────────────────────────────────
public sealed record GetPaymentByIdQuery(Guid Id) : IQuery<PaymentDetailDto>;
 
public sealed class GetPaymentByIdHandler(RetailStoreDbContext db)
    : IRequestHandler<GetPaymentByIdQuery, PaymentDetailDto>
{
    public async Task<PaymentDetailDto> Handle(GetPaymentByIdQuery q, CancellationToken ct)
    {
        var p = await db.Set<Payment>().AsNoTracking().Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.Id == q.Id, ct)
            ?? throw new DomainException(PaymentErrors.NotFound(q.Id));
 
        return new PaymentDetailDto(
            p.Id, p.OrderId, p.CustomerId,
            p.Amount, p.Currency, p.Method.ToString(), p.MethodDetail,
            p.Status.ToString(), p.GatewayName, p.GatewayTransactionId,
            p.AuthorizedAt, p.CapturedAt, p.FailedAt, p.RefundedAt, p.CancelledAt,
            p.FailureReason, p.Notes, p.TotalRefunded, p.NetAmount,
            p.Refunds.Select(r => new RefundDto(
                r.Id, r.Amount, r.Currency,
                r.Reason, r.Status.ToString(), r.ProcessedAt)).ToList(),
            p.CreatedAt);
    }
}
 
// ─── Get Payment By Order ───────────────────────────────────
public sealed record GetPaymentByOrderQuery(Guid OrderId) : IQuery<PaymentDetailDto>;
 
public sealed class GetPaymentByOrderHandler(RetailStoreDbContext db)
    : IRequestHandler<GetPaymentByOrderQuery, PaymentDetailDto>
{
    public async Task<PaymentDetailDto> Handle(GetPaymentByOrderQuery q, CancellationToken ct)
    {
        var p = await db.Set<Payment>().AsNoTracking().Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.OrderId == q.OrderId, ct)
            ?? throw new DomainException(PaymentErrors.NotFoundByOrder(q.OrderId));
 
        return new PaymentDetailDto(
            p.Id, p.OrderId, p.CustomerId,
            p.Amount, p.Currency, p.Method.ToString(), p.MethodDetail,
            p.Status.ToString(), p.GatewayName, p.GatewayTransactionId,
            p.AuthorizedAt, p.CapturedAt, p.FailedAt, p.RefundedAt, p.CancelledAt,
            p.FailureReason, p.Notes, p.TotalRefunded, p.NetAmount,
            p.Refunds.Select(r => new RefundDto(
                r.Id, r.Amount, r.Currency,
                r.Reason, r.Status.ToString(), r.ProcessedAt)).ToList(),
            p.CreatedAt);
    }
}