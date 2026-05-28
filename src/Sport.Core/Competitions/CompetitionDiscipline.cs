using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Core.Competitions;

public sealed class CompetitionDiscipline
{
    public CompetitionDisciplineId Id { get; }
    public CompetitionId CompetitionId { get; }
    public DisciplineCode Code { get; }
    public IReadOnlySet<GenderCode> EnabledGenders { get; }

    private CompetitionDiscipline(
        CompetitionDisciplineId id,
        CompetitionId competitionId,
        DisciplineCode code,
        IReadOnlySet<GenderCode> enabledGenders)
    {
        Id = id;
        CompetitionId = competitionId;
        Code = code;
        EnabledGenders = enabledGenders;
    }

    public static CompetitionDiscipline Create(
        CompetitionDisciplineId id,
        CompetitionId competitionId,
        DisciplineCode code,
        IReadOnlySet<GenderCode> enabledGenders)
    {
        if (enabledGenders is null || enabledGenders.Count == 0)
            throw new DomainException("A CompetitionDiscipline must enable at least one gender.");
        return new CompetitionDiscipline(id, competitionId, code, enabledGenders);
    }
}
