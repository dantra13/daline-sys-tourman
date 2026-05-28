using Vogen;

namespace Sport.Core.Structure;

[ValueObject<Guid>]
public readonly partial struct SubunitId
{
    public static SubunitId New() => From(Guid.CreateVersion7());
}
