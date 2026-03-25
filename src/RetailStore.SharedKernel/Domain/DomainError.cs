namespace RetailStore.SharedKernel.Domain;

/// <summary>
/// Immutable error descriptor. Carries everything needed to produce
/// a ProblemDetails response without any infrastructure knowledge.
/// </summary>
public sealed record DomainError(
    string Code,           // Machine-readable: "PRODUCT_NOT_FOUND"
    string Message,        // Human-readable: "Product with ID '...' does not exist"
    DomainErrorType Type   // Maps to HTTP status code automatically
);

public enum DomainErrorType
{
    Validation = 400,        // 400 Bad Request
    NotFound = 404,          // 404 Not Found
    Conflict = 409,          // 409 Conflict
    Unauthorized = 401,      // 401 Unauthorized
    Forbidden = 403,         // 403 Forbidden
    BusinessRule = 422,      // 422 Unprocessable Entity
    Internal = 500           // 500 Internal Server Error
}
