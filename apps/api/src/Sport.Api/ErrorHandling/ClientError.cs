namespace Sport.Api.ErrorHandling;

public sealed record ClientError(string Code, string Title, int Status);
