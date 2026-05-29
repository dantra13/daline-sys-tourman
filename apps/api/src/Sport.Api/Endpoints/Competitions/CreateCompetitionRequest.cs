using Sport.Application.Features.Competitions.CreateCompetition;

namespace Sport.Api.Endpoints.Competitions;

public sealed record CreateCompetitionRequest(
    string Code,
    string Name,
    CreateCompetitionRequest.DatesDto Dates,
    IReadOnlyList<CreateCompetitionRequest.DisciplineDto> Disciplines)
{
    public sealed record DatesDto(DateOnly Start, DateOnly End);
    public sealed record DisciplineDto(string Code, IReadOnlyList<string> Genders);

    public CreateCompetition ToCommand() => new(
        Code: Code,
        Name: Name,
        StartDate: Dates.Start,
        EndDate: Dates.End,
        Disciplines: Disciplines
            .Select(d => new CreateCompetition.DisciplineInput(d.Code, d.Genders))
            .ToList());
}
