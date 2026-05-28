using Vogen;

namespace Sport.Core.Structure;

[ValueObject<Guid>]
public readonly partial struct EventId
{
    public static EventId New() => From(Guid.CreateVersion7());
}
