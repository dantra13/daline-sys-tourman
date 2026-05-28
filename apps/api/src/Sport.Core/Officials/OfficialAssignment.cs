using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Shared;

namespace Sport.Core.Officials;

public sealed class OfficialAssignment
{
    public OfficialAssignmentId Id { get; }
    public PersonId PersonId { get; }
    public FunctionCode FunctionCode { get; }
    public OfficialScope Scope { get; }
    public OrganisationId? OrganisationId { get; }
    public OfficialAssignmentStatus Status { get; private set; }

    // Required by EF Core materializer (cannot bind ComplexProperty params).
#pragma warning disable CS8618
    private OfficialAssignment() { }
#pragma warning restore CS8618

    private OfficialAssignment(
        OfficialAssignmentId id, PersonId personId, FunctionCode functionCode,
        OfficialScope scope, OrganisationId? organisationId)
    {
        Id = id; PersonId = personId; FunctionCode = functionCode;
        Scope = scope; OrganisationId = organisationId;
        Status = OfficialAssignmentStatus.Active;
    }

    public static OfficialAssignment Create(
        OfficialAssignmentId id,
        PersonId personId,
        FunctionDescriptor descriptor,
        OfficialScope scope,
        OrganisationId? organisationId)
    {
        if (!descriptor.ValidScopes.Contains(scope.Level))
            throw new DomainException($"ScopeLevel '{scope.Level}' is not allowed for function '{descriptor.Code.Value}' (I-OFF-2).");

        if (descriptor.RequiresOrganisation && organisationId is null)
            throw new DomainException($"Function '{descriptor.Code.Value}' Organisation is required (I-OFF-3).");

        return new OfficialAssignment(id, personId, descriptor.Code, scope, organisationId);
    }

    public void Replace() => Status = OfficialAssignmentStatus.Replaced;
    public void Remove()  => Status = OfficialAssignmentStatus.Removed;
}
