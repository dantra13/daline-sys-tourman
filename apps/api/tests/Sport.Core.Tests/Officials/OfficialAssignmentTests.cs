using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Participants;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Officials;

public class OfficialAssignmentTests
{
    private static readonly FunctionDescriptor CoachDesc = new(
        FunctionCode.From("COMMON.COACH"),
        "Coach",
        new HashSet<ScopeLevel> { ScopeLevel.Event, ScopeLevel.Unit },
        IsTeamOfficial: true,
        RequiresOrganisation: true);

    private static readonly FunctionDescriptor RefDesc = new(
        FunctionCode.From("FBL.REF"),
        "Referee",
        new HashSet<ScopeLevel> { ScopeLevel.Unit },
        IsTeamOfficial: false,
        RequiresOrganisation: false);

    [Fact]
    public void Create_with_valid_descriptor_and_scope_succeeds()
    {
        var a = OfficialAssignment.Create(
            OfficialAssignmentId.New(),
            PersonId.New(),
            RefDesc,
            OfficialScope.Create(ScopeLevel.Unit, Guid.NewGuid()),
            organisationId: null);

        a.Status.Should().Be(OfficialAssignmentStatus.Active);
    }

    [Fact]
    public void Create_rejects_scope_level_not_in_valid_scopes_I_OFF_2()
    {
        var act = () => OfficialAssignment.Create(
            OfficialAssignmentId.New(),
            PersonId.New(),
            RefDesc,
            OfficialScope.Create(ScopeLevel.Competition, Guid.NewGuid()),
            organisationId: null);
        act.Should().Throw<DomainException>().WithMessage("*ScopeLevel*not*allowed*");
    }

    [Fact]
    public void Create_requires_organisation_when_descriptor_demands_it_I_OFF_3()
    {
        var act = () => OfficialAssignment.Create(
            OfficialAssignmentId.New(),
            PersonId.New(),
            CoachDesc,
            OfficialScope.Create(ScopeLevel.Event, Guid.NewGuid()),
            organisationId: null);
        act.Should().Throw<DomainException>().WithMessage("*Organisation*required*");
    }
}
