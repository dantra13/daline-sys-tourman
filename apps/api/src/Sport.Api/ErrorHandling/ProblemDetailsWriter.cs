using System.Diagnostics;
using System.Text.Json;

namespace Sport.Api.ErrorHandling;

internal static class ProblemDetailsWriter
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public sealed record ErrorEntry(
        string Code,
        string? Target,
        IReadOnlyDictionary<string, object?>? Params);

    public static async Task WriteAsync(
        HttpContext context,
        int status,
        string code,
        string title,
        string detail,
        IReadOnlyList<ErrorEntry> errors,
        CancellationToken ct)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var payload = new
        {
            type = $"https://daline.sys/errors/{code}",
            title,
            status,
            detail,
            code,
            errors = errors.Select(e => new
            {
                code = e.Code,
                target = e.Target,
                @params = e.Params,
            }),
            traceId,
        };

        await JsonSerializer.SerializeAsync(context.Response.Body, payload, JsonOpts, ct);
    }
}
