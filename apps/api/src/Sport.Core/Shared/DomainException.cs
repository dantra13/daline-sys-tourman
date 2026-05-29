namespace Sport.Core.Shared;

public sealed class DomainException : Exception
{
    public string Code { get; }

    public IReadOnlyDictionary<string, object?> Params { get; }

    public DomainException(
        string code,
        string message,
        IReadOnlyDictionary<string, object?>? @params = null)
        : base(message)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("DomainException.Code is required.", nameof(code));

        Code = code;
        Params = @params ?? new Dictionary<string, object?>();
    }
}
