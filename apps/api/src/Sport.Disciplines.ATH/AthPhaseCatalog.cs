using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Disciplines.ATH;

internal sealed class AthPhaseCatalog : IPhaseCatalog
{
    private static readonly HashSet<string> AllowedCodes = new(StringComparer.Ordinal)
    {
        "QUAL","FNL",
    };

    public IReadOnlyCollection<PhaseCode> AllowedPhases { get; } =
        AllowedCodes.Select(PhaseCode.From).ToArray();

    public bool IsAllowedForEventType(EventTypeCode eventType, PhaseCode phase) =>
        AllowedCodes.Contains(phase.Value);
}
