using Vogen;

namespace Sport.Core.Participants;

[ValueObject<Guid>]
public readonly partial struct TeamId
{
    public static TeamId New() => From(Guid.CreateVersion7());
}
