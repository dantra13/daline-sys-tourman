using Vogen;

namespace Sport.Core.Participants;

[ValueObject<Guid>]
public readonly partial struct EntryId
{
    public static EntryId New() => From(Guid.CreateVersion7());
}
