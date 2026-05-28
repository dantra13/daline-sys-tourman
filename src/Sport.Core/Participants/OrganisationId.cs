using Vogen;

namespace Sport.Core.Participants;

[ValueObject<Guid>]
public readonly partial struct OrganisationId
{
    public static OrganisationId New() => From(Guid.CreateVersion7());
}
