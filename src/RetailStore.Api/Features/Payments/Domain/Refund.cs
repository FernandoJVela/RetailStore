using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Payments.Domain;
 
public sealed class Refund : Entity
{
    public Guid PaymentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public string Reason { get; private set; } = string.Empty;
    public RefundStatus Status { get; private set; }
    public string? GatewayRefundId { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? FailureReason { get; private set; }
 
    private Refund() { } // EF Core
 
    internal static Refund Create(Guid paymentId, decimal amount, string currency, string reason)
    {
        if (amount <= 0)
            throw new DomainException(PaymentErrors.InvalidRefundAmount());
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException(PaymentErrors.RefundReasonRequired());
 
        return new Refund
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            Amount = amount,
            Currency = currency,
            Reason = reason.Trim(),
            Status = RefundStatus.Pending
        };
    }
 
    internal void MarkProcessing(string gatewayRefundId)
    {
        Status = RefundStatus.Processing;
        GatewayRefundId = gatewayRefundId;
        Touch();
    }
 
    internal void MarkCompleted()
    {
        Status = RefundStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
        Touch();
    }
 
    internal void MarkFailed(string reason)
    {
        Status = RefundStatus.Failed;
        FailedAt = DateTime.UtcNow;
        FailureReason = reason;
        Touch();
    }
}