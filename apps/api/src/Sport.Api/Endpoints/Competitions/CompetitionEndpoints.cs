using Sport.Application.Features.Competitions;
using Sport.Application.Features.Competitions.GetCompetition;
using Sport.Application.Features.Competitions.ListCompetitions;
using Wolverine;

namespace Sport.Api.Endpoints.Competitions;

public static class CompetitionEndpoints
{
    public static IEndpointRouteBuilder MapCompetitionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/competitions").WithTags("Competitions");

        group.MapPost("/", async (
            CreateCompetitionRequest req,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var dto = await bus.InvokeAsync<CompetitionDto>(req.ToCommand(), ct);
            return Results.Created($"/competitions/{dto.Id}", dto);
        });

        group.MapGet("/", async (IMessageBus bus, CancellationToken ct) =>
        {
            var items = await bus.InvokeAsync<IReadOnlyList<CompetitionDto>>(
                new ListCompetitions(), ct);
            return Results.Ok(new { items });
        });

        group.MapGet("/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
        {
            var dto = await bus.InvokeAsync<CompetitionDto>(new GetCompetition(id), ct);
            return Results.Ok(dto);
        });

        return app;
    }
}
