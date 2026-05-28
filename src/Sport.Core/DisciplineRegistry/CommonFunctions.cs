using Sport.Core.Officials;

namespace Sport.Core.DisciplineRegistry;

public static class CommonFunctions
{
    public static FunctionDescriptor Coach { get; } = new(
        FunctionCode.From("COMMON.COACH"),
        "Coach",
        new HashSet<ScopeLevel> { ScopeLevel.Event, ScopeLevel.Unit, ScopeLevel.CompetitionDiscipline },
        IsTeamOfficial: true,
        RequiresOrganisation: true);

    public static FunctionDescriptor Manager { get; } = new(
        FunctionCode.From("COMMON.MANAGER"),
        "Manager",
        new HashSet<ScopeLevel> { ScopeLevel.Event, ScopeLevel.CompetitionDiscipline, ScopeLevel.Competition },
        IsTeamOfficial: true,
        RequiresOrganisation: true);

    public static FunctionDescriptor Medical { get; } = new(
        FunctionCode.From("COMMON.MEDICAL"),
        "Medical Officer",
        new HashSet<ScopeLevel> { ScopeLevel.Event, ScopeLevel.Unit, ScopeLevel.Competition },
        IsTeamOfficial: true,
        RequiresOrganisation: false);

    public static IReadOnlyCollection<FunctionDescriptor> All { get; } =
        new[] { Coach, Manager, Medical };
}
