using Vogen;

namespace Sport.Core.Officials;

[ValueObject<Guid>]
public readonly partial struct OfficialAssignmentId
{
    public static OfficialAssignmentId New() => From(Guid.CreateVersion7());
}
