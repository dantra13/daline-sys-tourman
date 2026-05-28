using FluentAssertions;
using Sport.Core.Participants;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Tests.Participants;

public class EntryTests
{
    private static readonly EventId Ev = EventId.New();
    private static readonly OrganisationId Org = OrganisationId.New();
    private static readonly TeamId TeamA = TeamId.New();

    [Fact]
    public void Athlete_entry_must_have_exactly_one_composition_member_I_PAR_1()
    {
        var act = () => Entry.Create(EntryId.New(), Ev, EntryType.Athlete, Org, teamId: null,
            bib: null, seed: null,
            new[] { (PersonId.New(), 1, (Bib?)null), (PersonId.New(), 2, (Bib?)null) });
        act.Should().Throw<DomainException>().WithMessage("*exactly 1*");
    }

    [Fact]
    public void Team_entry_requires_team_id_I_PAR_2()
    {
        var act = () => Entry.Create(EntryId.New(), Ev, EntryType.Team, Org, teamId: null,
            bib: null, seed: null,
            new[] { (PersonId.New(), 1, (Bib?)null) });
        act.Should().Throw<DomainException>().WithMessage("*TeamId*");
    }

    [Fact]
    public void Group_entry_must_have_at_least_two_members_I_PAR_1()
    {
        var act = () => Entry.Create(EntryId.New(), Ev, EntryType.Group, Org, teamId: null,
            bib: null, seed: null,
            new[] { (PersonId.New(), 1, (Bib?)null) });
        act.Should().Throw<DomainException>().WithMessage("*at least 2*");
    }

    [Fact]
    public void Group_entry_rejects_team_id_I_PAR_2()
    {
        var act = () => Entry.Create(EntryId.New(), Ev, EntryType.Group, Org, teamId: TeamA,
            bib: null, seed: null,
            new[] { (PersonId.New(), 1, (Bib?)null), (PersonId.New(), 2, (Bib?)null) });
        act.Should().Throw<DomainException>().WithMessage("*Group*TeamId*null*");
    }

    [Fact]
    public void Duplicate_order_in_composition_throws_I_PAR_6()
    {
        var act = () => Entry.Create(EntryId.New(), Ev, EntryType.Team, Org, teamId: TeamA,
            bib: null, seed: null,
            new[]
            {
                (PersonId.New(), 1, (Bib?)null),
                (PersonId.New(), 1, (Bib?)null),
            });
        act.Should().Throw<DomainException>().WithMessage("*Order*unique*");
    }

    [Fact]
    public void Initial_status_is_Registered_I_PAR_8()
    {
        var e = Entry.Create(EntryId.New(), Ev, EntryType.Athlete, Org, teamId: null,
            bib: null, seed: null,
            new[] { (PersonId.New(), 1, (Bib?)null) });
        e.Status.Should().Be(EntryStatus.Registered);
    }
}
