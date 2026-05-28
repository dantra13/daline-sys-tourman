using Sport.Core.Participants;
using Sport.Core.Shared;

namespace Sport.Core.DisciplineRegistry;

public interface IEntryRules
{
    IReadOnlyCollection<EntryType> AllowedTypes { get; }
    (int Min, int Max) CompositionSize(EntryType type);
    Result Validate(EntryCandidate candidate);
}
