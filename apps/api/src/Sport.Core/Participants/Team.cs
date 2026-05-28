using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Core.Participants;

public sealed class Team
{
    public TeamId Id { get; }
    public TeamCode Code { get; }
    public string Name { get; }
    public OrganisationId OrganisationId { get; }
    public DisciplineCode DisciplineCode { get; }

    private Team(TeamId id, TeamCode code, string name, OrganisationId organisationId, DisciplineCode disciplineCode)
    {
        Id = id; Code = code; Name = name; OrganisationId = organisationId; DisciplineCode = disciplineCode;
    }

    public static Team Create(TeamId id, TeamCode code, string name, OrganisationId organisationId, DisciplineCode disciplineCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Team.Name is required.");
        return new Team(id, code, name, organisationId, disciplineCode);
    }
}
