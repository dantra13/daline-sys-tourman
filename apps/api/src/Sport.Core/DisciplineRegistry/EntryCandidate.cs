using Sport.Core.Participants;

namespace Sport.Core.DisciplineRegistry;

public sealed record EntryCandidate(
    EntryType Type,
    int CompositionSize,
    bool HasTeam,
    bool HasOrganisation);
