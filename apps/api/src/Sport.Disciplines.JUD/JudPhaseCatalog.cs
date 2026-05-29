using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD;

// Provisional phase allow-list. Phases are really a competition-format concern; this declares
// judo's typical single-elimination + repechage shape until the format subsystem exists.
public sealed class JudPhaseCatalog : IPhaseCatalog
{
    private static readonly HashSet<string> Individual = new(StringComparer.Ordinal)
    {
        "R64", "R32", "8FNL", "QFNL", "SFNL", "FNL", "REP1", "REPF",
    };

    private static readonly HashSet<string> Team = new(StringComparer.Ordinal)
    {
        "R32", "8FNL", "QFNL", "SFNL", "FNL",
    };

    public IReadOnlyCollection<PhaseCode> AllowedPhases { get; } =
        Individual.Union(Team).Select(PhaseCode.From).ToArray();

    public bool IsAllowedForEventType(EventTypeCode eventType, PhaseCode phase) =>
        eventType.Value.StartsWith("TEAM", StringComparison.Ordinal)
            ? Team.Contains(phase.Value)
            : Individual.Contains(phase.Value);
}
