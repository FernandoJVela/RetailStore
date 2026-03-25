namespace RetailStore.SharedKernel.Domain;

/// <summary>
/// The ONLY exception type thrown by domain/application code.
/// The global exception handler knows how to convert this to ProblemDetails.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainError Error { get; }
    public IReadOnlyList<DomainError>? ValidationErrors { get; }

    public DomainException(DomainError error)
        : base(error.Message)
    {
        Error = error;
    }

    /// <summary>
    /// Used by the validation pipeline for aggregate validation failures.
    /// </summary>
    public DomainException(IReadOnlyList<DomainError> validationErrors)
        : base("One or more validation errors occurred.")
    {
        Error = new DomainError(
            "VALIDATION_FAILED",
            "One or more validation errors occurred.",
            DomainErrorType.Validation);
        ValidationErrors = validationErrors;
    }
}
