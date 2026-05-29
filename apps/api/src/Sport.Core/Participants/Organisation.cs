using Sport.Core.Shared;

namespace Sport.Core.Participants;

public sealed class Organisation
{
    public OrganisationId Id { get; }
    public OrganisationCode Code { get; }
    public string Name { get; }
    public OrganisationType Type { get; }

    private Organisation(OrganisationId id, OrganisationCode code, string name, OrganisationType type)
    {
        Id = id; Code = code; Name = name; Type = type;
    }

    public static Organisation Create(OrganisationId id, OrganisationCode code, string name, OrganisationType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("I-PAR-3", "Organisation.Name is required.");
        return new Organisation(id, code, name, type);
    }
}
