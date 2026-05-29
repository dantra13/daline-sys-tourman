using Sport.Core.Competitions;

namespace Sport.Application.Features.Competitions;

public sealed record CompetitionDto(
    Guid Id,
    string Code,
    string Name,
    CompetitionDto.DateRangeDto Dates,
    IReadOnlyList<CompetitionDto.DisciplineDto> Disciplines)
{
    public sealed record DateRangeDto(DateOnly Start, DateOnly End);
    public sealed record DisciplineDto(Guid Id, string Code, IReadOnlyList<string> Genders);

    public static CompetitionDto From(Competition c) => new(
        Id: c.Id.Value,
        Code: c.Code.Value,
        Name: c.Name,
        Dates: new DateRangeDto(c.Dates.Start, c.Dates.End),
        Disciplines: c.Disciplines
            .Select(d => new DisciplineDto(
                Id: d.Id.Value,
                Code: d.Code.Value,
                Genders: d.EnabledGenders.Select(g => g.ToString()).ToList()))
            .ToList());
}
