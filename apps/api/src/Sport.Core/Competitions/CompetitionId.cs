using Vogen;

namespace Sport.Core.Competitions;

[ValueObject<Guid>]
public readonly partial struct CompetitionId
{
    public static CompetitionId New() => From(Guid.CreateVersion7());
}
