using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;

namespace Sport.Application.Tests.Fakes;

internal static class CompetitionFactory
{
    public static Competition JudOpen(IDisciplineRegistry registry) =>
        Competition.Create(
            id: CompetitionId.From(Guid.NewGuid()),
            code: CompetitionCode.From("jud-2026"),
            name: "Judo Open 2026",
            dates: DateRange.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 5)),
            disciplines: new[]
            {
                (DisciplineCode.From("FBL"),
                    (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }),
            },
            registry: registry);
}
