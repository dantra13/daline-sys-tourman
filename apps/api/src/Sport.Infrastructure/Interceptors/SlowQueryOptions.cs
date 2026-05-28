namespace Sport.Infrastructure.Interceptors;

public sealed class SlowQueryOptions
{
    public int ThresholdMs { get; set; } = 200;
}
