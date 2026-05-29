using Sport.Core.Structure;

namespace Sport.Core.DisciplineRegistry;

public sealed record EventTypeDescriptor(
    EventTypeCode Code,
    string DisplayName,
    IReadOnlySet<GenderCode> AppliesToGenders,
    ModifierContract ModifierContract)
{
    public bool HostsSubunits { get; init; }
    public IReadOnlyCollection<SubunitCode> CanonicalSubunits { get; init; } = Array.Empty<SubunitCode>();
}
