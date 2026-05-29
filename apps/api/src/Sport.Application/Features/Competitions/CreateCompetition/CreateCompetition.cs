namespace Sport.Application.Features.Competitions.CreateCompetition;

public sealed record CreateCompetition(
    string Code,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<CreateCompetition.DisciplineInput> Disciplines)
{
    public sealed record DisciplineInput(string Code, IReadOnlyList<string> Genders);
}
