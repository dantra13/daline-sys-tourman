namespace Sport.Application.Common;

public sealed class NotFoundException : Exception
{
    public string Code { get; }
    public IReadOnlyDictionary<string, object?> Params { get; }

    public NotFoundException(string code, IReadOnlyDictionary<string, object?>? @params = null)
        : base($"Resource not found ({code}).")
    {
        Code = code;
        Params = @params ?? new Dictionary<string, object?>();
    }
}
