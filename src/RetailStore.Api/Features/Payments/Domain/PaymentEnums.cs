namespace RetailStore.Api.Features.Payments.Domain;
 
public enum PaymentStatus
{
    Pending,
    Authorized,
    Captured,
    Failed,
    Refunded,
    PartialRefund,
    Cancelled,
    Expired
}
 
public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    BankTransfer,
    Cash,
    DigitalWallet,
    PSE     // Colombian online bank transfer
}
 
public enum RefundStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}