using Microsoft.Extensions.Logging;
using Sport.Application.Common;
using Sport.Core.Shared;

namespace Sport.Api.ErrorHandling;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _log;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (ValidationException vex)
        {
            await HandleValidationAsync(ctx, vex);
        }
        catch (NotFoundException nfx)
        {
            await ProblemDetailsWriter.WriteAsync(
                ctx,
                status: 404,
                code: nfx.Code,
                title: "Resource not found.",
                detail: nfx.Message,
                errors: new[]
                {
                    new ProblemDetailsWriter.ErrorEntry(nfx.Code, null, nfx.Params),
                },
                ct: ctx.RequestAborted);
        }
        catch (DomainException dex)
        {
            await HandleDomainAsync(ctx, dex);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unhandled exception in pipeline.");
            await ProblemDetailsWriter.WriteAsync(
                ctx,
                status: 500,
                code: "internal.unexpected",
                title: "An unexpected error occurred.",
                detail: "Unexpected server error.",
                errors: new[]
                {
                    new ProblemDetailsWriter.ErrorEntry("internal.unexpected", null, null),
                },
                ct: ctx.RequestAborted);
        }
    }

    private static async Task HandleValidationAsync(HttpContext ctx, ValidationException vex)
    {
        // 409 if and only if the single failure is the uniqueness conflict.
        var isConflict =
            vex.Failures.Count == 1 &&
            vex.Failures[0].Code == "competition.code_already_exists";

        var status = isConflict ? 409 : 422;
        var top = vex.Failures[0];
        var entries = vex.Failures
            .Select(f => new ProblemDetailsWriter.ErrorEntry(f.Code, f.Target, f.Params))
            .ToArray();

        await ProblemDetailsWriter.WriteAsync(
            ctx,
            status: status,
            code: top.Code,
            title: isConflict ? "Resource conflict." : "Request validation failed.",
            detail: vex.Message,
            errors: entries,
            ct: ctx.RequestAborted);
    }

    private static async Task HandleDomainAsync(HttpContext ctx, DomainException dex)
    {
        var mapped = DomainErrorCatalog.TryGet(dex.Code);
        if (mapped is null)
        {
            // Domain code without catalog entry — shouldn't happen (arch test enforces it).
            await ProblemDetailsWriter.WriteAsync(
                ctx,
                status: 500,
                code: "internal.unexpected",
                title: "An unexpected error occurred.",
                detail: $"Unmapped domain code: {dex.Code}.",
                errors: new[]
                {
                    new ProblemDetailsWriter.ErrorEntry("internal.unexpected", null, null),
                },
                ct: ctx.RequestAborted);
            return;
        }

        await ProblemDetailsWriter.WriteAsync(
            ctx,
            status: mapped.Status,
            code: mapped.Code,
            title: mapped.Title,
            detail: dex.Message,
            errors: new[]
            {
                new ProblemDetailsWriter.ErrorEntry(mapped.Code, null, dex.Params),
            },
            ct: ctx.RequestAborted);
    }
}
