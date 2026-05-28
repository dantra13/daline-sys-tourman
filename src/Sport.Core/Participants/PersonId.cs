using Vogen;

namespace Sport.Core.Participants;

[ValueObject<Guid>]
public readonly partial struct PersonId
{
    public static PersonId New() => From(Guid.CreateVersion7());
}
