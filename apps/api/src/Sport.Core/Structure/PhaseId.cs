using Vogen;

namespace Sport.Core.Structure;

[ValueObject<Guid>]
public readonly partial struct PhaseId
{
    public static PhaseId New() => From(Guid.CreateVersion7());
}
