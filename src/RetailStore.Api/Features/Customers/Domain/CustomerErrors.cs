using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Customers.Domain;
 
public static class CustomerErrors
{
    // ─── Lookup ────────────────────────────────────────────
    public static DomainError NotFound(Guid id) => new(
        "CUSTOMER_NOT_FOUND",
        $"Customer with ID '{id}' does not exist.",
        DomainErrorType.NotFound);
 
    public static DomainError NotFoundByEmail(string email) => new(
        "CUSTOMER_NOT_FOUND",
        $"No customer with email '{email}'.",
        DomainErrorType.NotFound);
 
    public static DomainError NoCustomersFound() => new(
        "CUSTOMER_LIST_EMPTY",
        "No customers match the search criteria.",
        DomainErrorType.NotFound);
 
    // ─── Conflict ──────────────────────────────────────────
    public static DomainError DuplicateEmail(string email) => new(
        "CUSTOMER_DUPLICATE_EMAIL",
        $"A customer with email '{email}' already exists.",
        DomainErrorType.Conflict);
 
    public static DomainError AlreadyDeactivated() => new(
        "CUSTOMER_ALREADY_DEACTIVATED",
        "Customer is already deactivated.",
        DomainErrorType.Conflict);
 
    public static DomainError AlreadyActive() => new(
        "CUSTOMER_ALREADY_ACTIVE",
        "Customer is already active.",
        DomainErrorType.Conflict);
 
    // ─── Validation / Business Rules ───────────────────────
    public static DomainError InvalidEmail(string email) => new(
        "CUSTOMER_INVALID_EMAIL",
        $"'{email}' is not a valid email address.",
        DomainErrorType.Validation);
 
    public static DomainError InvalidName(string field) => new(
        "CUSTOMER_INVALID_NAME",
        $"{field} is required and cannot be empty.",
        DomainErrorType.Validation);
 
    public static DomainError InvalidPhone(string phone) => new(
        "CUSTOMER_INVALID_PHONE",
        $"'{phone}' is not a valid phone number.",
        DomainErrorType.Validation);
 
    public static DomainError InvalidAddress(string reason) => new(
        "CUSTOMER_INVALID_ADDRESS",
        $"Invalid address: {reason}",
        DomainErrorType.Validation);
 
    public static DomainError CannotDeleteWithOrders(Guid id) => new(
        "CUSTOMER_HAS_ORDERS",
        $"Customer '{id}' cannot be deleted because they have existing orders. Deactivate instead.",
        DomainErrorType.BusinessRule);
 
    public static DomainError InactiveCustomerCannotOrder(Guid id) => new(
        "CUSTOMER_INACTIVE_CANNOT_ORDER",
        $"Customer '{id}' is deactivated and cannot place orders.",
        DomainErrorType.BusinessRule);
}