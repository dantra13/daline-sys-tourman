using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Disciplines.BOX;

internal sealed class BoxPhaseCatalog : IPhaseCatalog
{
    private static readonly HashSet<string> AllowedCodes = new(StringComparer.Ordinal)
    {
        "R32","R16","QFNL","SFNL","FNL",
    };

    public IReadOnlyCollection<PhaseCode> AllowedPhases { get; } =
        AllowedCodes.Select(PhaseCode.From).ToArray();

    public bool IsAllowedForEventType(EventTypeCode eventType, PhaseCode phase) =>
        AllowedCodes.Contains(phase.Value);
}
