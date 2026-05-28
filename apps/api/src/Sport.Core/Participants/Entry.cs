using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Participants;

public sealed class Entry
{
    public EntryId Id { get; }
    public EventId EventId { get; }
    public EntryType Type { get; }
    public OrganisationId OrganisationId { get; }
    public TeamId? TeamId { get; }
    public Bib? Bib { get; }
    public int? Seed { get; }
    public EntryStatus Status { get; private set; }

    private readonly List<CompositionMember> _composition;
    public IReadOnlyList<CompositionMember> Composition => _composition;

    private Entry(
        EntryId id, EventId eventId, EntryType type, OrganisationId organisationId,
        TeamId? teamId, Bib? bib, int? seed, List<CompositionMember> composition)
    {
        Id = id; EventId = eventId; Type = type; OrganisationId = organisationId;
        TeamId = teamId; Bib = bib; Seed = seed;
        _composition = composition;
        Status = EntryStatus.Registered;
    }

    public static Entry Create(
        EntryId id,
        EventId eventId,
        EntryType type,
        OrganisationId organisationId,
        TeamId? teamId,
        Bib? bib,
        int? seed,
        IReadOnlyCollection<(PersonId PersonId, int Order, Bib? Bib)> members)
    {
        ValidateTeamRules(type, teamId);
        ValidateComposition(type, members);

        var composition = members
            .Select(m => new CompositionMember(id, m.PersonId, m.Order, m.Bib))
            .ToList();

        return new Entry(id, eventId, type, organisationId, teamId, bib, seed, composition);
    }

    public void Withdraw()   => Status = EntryStatus.Withdrawn;
    public void Disqualify() => Status = EntryStatus.Disqualified;
    public void Replace()    => Status = EntryStatus.Replaced;

    private static void ValidateTeamRules(EntryType type, TeamId? teamId)
    {
        switch (type)
        {
            case EntryType.Team when teamId is null:
                throw new DomainException("Type=Team requires TeamId (I-PAR-2).");
            case EntryType.Athlete when teamId is not null:
                throw new DomainException("Type=Athlete: TeamId must be null (I-PAR-2).");
            case EntryType.Group when teamId is not null:
                throw new DomainException("Type=Group: TeamId must be null (I-PAR-2).");
        }
    }

    private static void ValidateComposition(
        EntryType type,
        IReadOnlyCollection<(PersonId PersonId, int Order, Bib? Bib)> members)
    {
        if (members is null) throw new DomainException("Composition is required.");

        switch (type)
        {
            case EntryType.Athlete when members.Count != 1:
                throw new DomainException("Athlete entry must contain exactly 1 composition member (I-PAR-1).");
            case EntryType.Team when members.Count < 1:
                throw new DomainException("Team entry must contain at least 1 composition member (I-PAR-1).");
            case EntryType.Group when members.Count < 2:
                throw new DomainException("Group entry must contain at least 2 composition members (I-PAR-1).");
        }

        var seenOrders = new HashSet<int>();
        var seenPersons = new HashSet<PersonId>();
        foreach (var m in members)
        {
            if (!seenOrders.Add(m.Order))
                throw new DomainException("CompositionMember.Order must be unique within Entry (I-PAR-6).");
            if (!seenPersons.Add(m.PersonId))
                throw new DomainException("PersonId duplicated within the same Entry composition.");
        }
    }
}
