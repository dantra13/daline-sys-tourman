using Vogen;

namespace Sport.Core.Structure;

[ValueObject<Guid>]
public readonly partial struct UnitId
{
    public static UnitId New() => From(Guid.CreateVersion7());
}
