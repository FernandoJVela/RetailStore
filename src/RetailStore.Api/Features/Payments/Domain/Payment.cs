using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Payments.Domain;
 
public sealed class Payment : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public PaymentMethod Method { get; private set; }
    public string? MethodDetail { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? GatewayName { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? GatewayResponse { get; private set; }
    public DateTime? AuthorizedAt { get; private set; }
    public DateTime? CapturedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? FailureReason { get; private set; }
    public string? Notes { get; private set; }
 
    private readonly List<Refund> _refunds = new();
    public IReadOnlyCollection<Refund> Refunds => _refunds.AsReadOnly();
 
    private Payment() { } // EF Core
 
    // ─── Computed ───────────────────────────────────────────
    public decimal TotalRefunded => _refunds
        .Where(r => r.Status == RefundStatus.Completed)
        .Sum(r => r.Amount);
    public decimal NetAmount => Amount - TotalRefunded;
    public bool IsFullyRefunded => TotalRefunded >= Amount;
 
    // ─── Factory ────────────────────────────────────────────
    public static Payment Create(
        Guid orderId, Guid customerId,
        decimal amount, string currency,
        PaymentMethod method, string? methodDetail = null)
    {
        if (orderId == Guid.Empty)
            throw new DomainException(PaymentErrors.InvalidOrderId());
        if (customerId == Guid.Empty)
            throw new DomainException(PaymentErrors.InvalidCustomerId());
        if (amount <= 0)
            throw new DomainException(PaymentErrors.InvalidAmount());
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException(PaymentErrors.InvalidCurrency());
 
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = customerId,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Method = method,
            MethodDetail = methodDetail?.Trim(),
            Status = PaymentStatus.Pending
        };
 
        payment.Raise(new PaymentCreatedEvent(
            payment.Id, orderId, customerId, amount, currency, method.ToString()));
 
        return payment;
    }
 
    // ─── Gateway Integration ────────────────────────────────
    public void SetGateway(string gatewayName, string transactionId, string? rawResponse = null)
    {
        if (string.IsNullOrWhiteSpace(gatewayName))
            throw new DomainException(PaymentErrors.InvalidGateway());
 
        GatewayName = gatewayName.Trim();
        GatewayTransactionId = transactionId?.Trim();
        GatewayResponse = rawResponse;
        Touch();
    }
 
    // ─── Status Transitions ─────────────────────────────────
    public void Authorize()
    {
        if (Status != PaymentStatus.Pending)
            throw new DomainException(
                PaymentErrors.InvalidStatusTransition(Status, PaymentStatus.Authorized));
 
        Status = PaymentStatus.Authorized;
        AuthorizedAt = DateTime.UtcNow;
        Touch();
        IncrementVersion();
 
        Raise(new PaymentAuthorizedEvent(Id, OrderId, Amount, Currency, GatewayTransactionId));
    }
 
    public void Capture()
    {
        if (Status != PaymentStatus.Authorized)
            throw new DomainException(
                PaymentErrors.InvalidStatusTransition(Status, PaymentStatus.Captured));
 
        Status = PaymentStatus.Captured;
        CapturedAt = DateTime.UtcNow;
        Touch();
        IncrementVersion();
 
        Raise(new PaymentCapturedEvent(Id, OrderId, CustomerId, Amount, Currency));
    }
 
    public void Fail(string reason)
    {
        if (Status is PaymentStatus.Captured or PaymentStatus.Refunded)
            throw new DomainException(
                PaymentErrors.InvalidStatusTransition(Status, PaymentStatus.Failed));
 
        Status = PaymentStatus.Failed;
        FailedAt = DateTime.UtcNow;
        FailureReason = reason;
        Touch();
 
        Raise(new PaymentFailedEvent(Id, OrderId, reason));
    }
 
    public void Cancel(string? reason = null)
    {
        if (Status is PaymentStatus.Captured or PaymentStatus.Refunded)
            throw new DomainException(
                PaymentErrors.InvalidStatusTransition(Status, PaymentStatus.Cancelled));
 
        Status = PaymentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        Notes = reason;
        Touch();
 
        Raise(new PaymentCancelledEvent(Id, OrderId, reason));
    }
 
    public void Expire()
    {
        if (Status != PaymentStatus.Pending)
            throw new DomainException(
                PaymentErrors.InvalidStatusTransition(Status, PaymentStatus.Expired));
 
        Status = PaymentStatus.Expired;
        Touch();
 
        Raise(new PaymentExpiredEvent(Id, OrderId));
    }
 
    // ─── Refunds ────────────────────────────────────────────
    public Refund RequestRefund(decimal amount, string reason)
    {
        if (Status != PaymentStatus.Captured && Status != PaymentStatus.PartialRefund)
            throw new DomainException(PaymentErrors.CannotRefundUncaptured());
 
        if (amount > NetAmount)
            throw new DomainException(
                PaymentErrors.RefundExceedsPayment(amount, NetAmount));
 
        var refund = Refund.Create(Id, amount, Currency, reason);
        _refunds.Add(refund);
        Touch();
 
        Raise(new RefundRequestedEvent(Id, refund.Id, OrderId, amount, Currency, reason));
        return refund;
    }
 
    public void CompleteRefund(Guid refundId)
    {
        var refund = _refunds.FirstOrDefault(r => r.Id == refundId)
            ?? throw new DomainException(PaymentErrors.RefundNotFound(refundId));
 
        refund.MarkCompleted();
 
        if (IsFullyRefunded)
        {
            Status = PaymentStatus.Refunded;
            RefundedAt = DateTime.UtcNow;
            Raise(new PaymentFullyRefundedEvent(Id, OrderId, Amount, Currency));
        }
        else
        {
            Status = PaymentStatus.PartialRefund;
            Raise(new RefundCompletedEvent(Id, refund.Id, refund.Amount, Currency));
        }
 
        Touch();
        IncrementVersion();
    }
 
    public void FailRefund(Guid refundId, string reason)
    {
        var refund = _refunds.FirstOrDefault(r => r.Id == refundId)
            ?? throw new DomainException(PaymentErrors.RefundNotFound(refundId));
 
        refund.MarkFailed(reason);
        Touch();
 
        Raise(new RefundFailedEvent(Id, refund.Id, reason));
    }
 
    public void AddNote(string note)
    {
        Notes = string.IsNullOrEmpty(Notes) ? note : $"{Notes}\n{note}";
        Touch();
    }
}