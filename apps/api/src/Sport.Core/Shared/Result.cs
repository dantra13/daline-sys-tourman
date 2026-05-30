namespace Sport.Core.Shared;

public readonly record struct Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? Code { get; }

    private Result(bool isSuccess, string? error, string? code)
    {
        IsSuccess = isSuccess;
        Error = error;
        Code = code;
    }

    public static Result Ok() => new(true, null, null);

    public static Result Fail(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error message is required for a failure result.", nameof(error));
        return new Result(false, error, null);
    }

    public static Result Fail(string code, string error)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code is required for a failure result.", nameof(code));
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error message is required for a failure result.", nameof(error));
        return new Result(false, error, code);
    }
}
