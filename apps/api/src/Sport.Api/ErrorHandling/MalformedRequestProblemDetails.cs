using Microsoft.AspNetCore.Http;

namespace Sport.Api.ErrorHandling;

internal static class MalformedRequestProblemDetails
{
    public static IServiceCollection AddUnifiedProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(opts =>
        {
            opts.CustomizeProblemDetails = ctx =>
            {
                var status = ctx.ProblemDetails.Status ?? ctx.HttpContext.Response.StatusCode;
                var (code, title) = status switch
                {
                    400 => ("request.malformed",   "Malformed request."),
                    415 => ("request.unsupported", "Unsupported media type."),
                    _   => ("request.invalid",    ctx.ProblemDetails.Title ?? "Invalid request."),
                };

                ctx.ProblemDetails.Type   = $"https://daline.sys/errors/{code}";
                ctx.ProblemDetails.Title  = title;
                ctx.ProblemDetails.Extensions["code"] = code;
                ctx.ProblemDetails.Extensions["errors"] = new[]
                {
                    new { code, target = (string?)null, @params = (IReadOnlyDictionary<string, object?>?)null },
                };
                ctx.ProblemDetails.Extensions["traceId"] =
                    System.Diagnostics.Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier;
            };
        });

        return services;
    }
}
