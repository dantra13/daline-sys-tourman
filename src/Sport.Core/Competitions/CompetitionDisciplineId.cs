using Vogen;

namespace Sport.Core.Competitions;

[ValueObject<Guid>]
public readonly partial struct CompetitionDisciplineId
{
    public static CompetitionDisciplineId New() => From(Guid.CreateVersion7());
}
