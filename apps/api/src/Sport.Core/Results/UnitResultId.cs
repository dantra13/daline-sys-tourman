using Vogen;

namespace Sport.Core.Results;

[ValueObject<Guid>]
public readonly partial struct UnitResultId
{
    public static UnitResultId New() => From(Guid.CreateVersion7());
}
