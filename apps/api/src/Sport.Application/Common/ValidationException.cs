namespace Sport.Application.Common;

public sealed class ValidationException : Exception
{
    public IReadOnlyList<ValidationFailure> Failures { get; }

    public ValidationException(IReadOnlyList<ValidationFailure> failures)
        : base($"Validation failed with {failures.Count} failure(s).")
    {
        if (failures.Count == 0)
            throw new ArgumentException("ValidationException requires at least one failure.", nameof(failures));
        Failures = failures;
    }

    public ValidationException(ValidationFailure failure)
        : this(new[] { failure })
    {
    }
}
