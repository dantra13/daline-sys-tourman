using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Core.Competitions;

public sealed class CompetitionDiscipline
{
    public CompetitionDisciplineId Id { get; }
    public CompetitionId CompetitionId { get; }
    public DisciplineCode Code { get; }
    public IReadOnlyList<GenderCode> EnabledGenders { get; }

    // Private parameterless ctor needed by EF.
    private CompetitionDiscipline() { EnabledGenders = Array.Empty<GenderCode>(); }

    private CompetitionDiscipline(
        CompetitionDisciplineId id,
        CompetitionId competitionId,
        DisciplineCode code,
        IReadOnlyList<GenderCode> enabledGenders)
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
            throw new DomainException("I-COMP-6", "A CompetitionDiscipline must enable at least one gender.");
        // Store as array; dedup preserved because input is a set.
        return new CompetitionDiscipline(id, competitionId, code, enabledGenders.ToArray());
    }
}
