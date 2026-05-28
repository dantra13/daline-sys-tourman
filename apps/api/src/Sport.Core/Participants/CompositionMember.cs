namespace Sport.Core.Participants;

public sealed record CompositionMember(EntryId EntryId, PersonId PersonId, int Order, Bib? Bib);
