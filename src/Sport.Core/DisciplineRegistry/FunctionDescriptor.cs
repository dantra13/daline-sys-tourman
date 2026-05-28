using Sport.Core.Officials;

namespace Sport.Core.DisciplineRegistry;

public sealed record FunctionDescriptor(
    FunctionCode Code,
    string DisplayName,
    IReadOnlySet<ScopeLevel> ValidScopes,
    bool IsTeamOfficial,
    bool RequiresOrganisation);
