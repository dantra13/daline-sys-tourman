using Sport.Core.Shared;

namespace Sport.Core.Officials;

public readonly record struct OfficialScope
{
    public ScopeLevel Level { get; }
    public Guid TargetId { get; }

    private OfficialScope(ScopeLevel level, Guid targetId)
    {
        Level = level;
        TargetId = targetId;
    }

    public static OfficialScope Create(ScopeLevel level, Guid targetId)
    {
        if (targetId == Guid.Empty)
            throw new DomainException("I-OFF-1", "OfficialScope.TargetId must not be empty.");
        return new OfficialScope(level, targetId);
    }
}
