using Sport.Core.DisciplineRegistry;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD;

public sealed class JudResultSchema : DefaultResultSchema
{
    private static readonly ExtensionCode Comp = ExtensionCode.From("COMP");
    private static readonly ExtensionCode WeightCategory = ExtensionCode.From("WEIGHT_CATEGORY");
    private static readonly ExtensionType Team = ExtensionType.From("TEAM");
    private static readonly IReadOnlySet<Irm> JudIrms = new HashSet<Irm>
    {
        Irm.From("DNS"), Irm.From("DQB"), Irm.From("DSQ"), Irm.From("WDR"),
    };

    public override OutcomeMode OutcomeModeFor(EventTypeCode type) => OutcomeMode.HeadToHead;
    public override IReadOnlySet<Irm> IrmCodesFor(EventTypeCode type) => JudIrms;

    public override DisciplineResultProjection Project(UnitResultDocument document) =>
        new JudoTeamMatchResult(document.Extensions.Count(e => e.Code == Comp));

    public override ResultRollup AggregateSubunits(UnitResultDocument parent, IReadOnlyList<UnitResultDocument> contestResults)
    {
        // Structural guard (don't trust the caller): parent is unit-level, every contest is a subunit of THIS unit,
        // and each contest has two sides of one athlete.
        if (parent.TargetSubunitId is not null)
            throw new DomainException("I-RES-10", "AggregateSubunits requires a unit-level parent document.");
        foreach (var c in contestResults)
        {
            if (c.TargetSubunitId is null || c.TargetUnitId != parent.TargetUnitId)
                throw new DomainException("I-RES-2", "Each contest must be a subunit of the parent unit.");
            if (c.Competitors.Count != 2 || c.Competitors.Any(x => x.Composition.Count != 1))
                throw new DomainException("I-RES-2", "Each JUD contest must have two sides, each with one athlete.");
        }

        var homeTeam = parent.Competitors.Single(c => c.SortOrder == 1);
        var awayTeam = parent.Competitors.Single(c => c.SortOrder == 2);

        var homeWins = 0;
        var awayWins = 0;
        var extensions = new List<ResultExtension>();
        var pos = 1;

        foreach (var contest in contestResults)
        {
            var homeSide = contest.Competitors.Single(c => c.SortOrder == 1);
            var awaySide = contest.Competitors.Single(c => c.SortOrder == 2);
            if (homeSide.Wlt == Wlt.W) homeWins++;
            else if (awaySide.Wlt == Wlt.W) awayWins++;

            var children = new List<ResultExtension>();
            var weight = contest.Extensions.FirstOrDefault(e => e.Code == WeightCategory)?.Value;
            if (weight is not null)
                children.Add(new ResultExtension(Team, WeightCategory) { Value = weight });
            // ODF HOME/AWAY are athlete IDs: the contest side's single composition member.
            children.Add(new ResultExtension(Team, ExtensionCode.From("HOME")) { Value = homeSide.Composition.Single().PersonId.Value.ToString() });
            children.Add(new ResultExtension(Team, ExtensionCode.From("AWAY")) { Value = awaySide.Composition.Single().PersonId.Value.ToString() });

            extensions.Add(new ResultExtension(Team, Comp)
            {
                Pos = pos.ToString(),
                Value = contest.TargetRsc.Value,
                Children = children,
            });
            pos++;
        }

        // Decisive => winner; genuine tie => unresolved (golden score is just an extra decisive contest).
        (Wlt? homeWlt, Wlt? awayWlt) =
            homeWins > awayWins ? (Wlt.W, Wlt.L) :
            awayWins > homeWins ? (Wlt.L, Wlt.W) :
            ((Wlt?)null, (Wlt?)null);

        var resolved = homeWlt is not null;
        var allOfficial = contestResults.Count > 0 && contestResults.All(c => c.Status == ResultStatus.Official);
        ResultStatus? suggested = resolved && allOfficial ? ResultStatus.Official : null;

        var competitors = new[]
        {
            homeTeam with { ResultValue = homeWins.ToString(), Wlt = homeWlt, ResultType = ResultTypeCode.From("POINTS") },
            awayTeam with { ResultValue = awayWins.ToString(), Wlt = awayWlt, ResultType = ResultTypeCode.From("POINTS") },
        };

        return new ResultRollup(competitors, extensions, suggested);
    }
}
