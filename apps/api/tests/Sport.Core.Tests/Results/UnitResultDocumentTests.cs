using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Tests.Results;

public class UnitResultDocumentTests
{
    private static readonly DisciplineCode Disc = DisciplineCode.From("FBL");
    private static readonly EventTypeCode Type = EventTypeCode.From("TEAM11");
    private static readonly Rsc UnitRsc = Rsc.From("FBLMTEAM11------------FNL-000100--");
    private static readonly IResultSchema Schema = new DefaultResultSchema();

    private static UnitResultDocument NewDoc() =>
        UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, Disc, Type);

    [Fact]
    public void CreateForUnit_starts_in_StartList_with_no_subunit_and_version_1()
    {
        var doc = NewDoc();
        doc.Status.Should().Be(ResultStatus.StartList);
        doc.TargetSubunitId.Should().BeNull();
        doc.Version.Should().Be(1);
    }

    [Fact]
    public void CreateForSubunit_sets_target_subunit()
    {
        var unitId = UnitId.New();
        var doc = UnitResultDocument.CreateForSubunit(UnitResultId.New(), unitId, SubunitId.New(), UnitRsc, Disc, Type);
        doc.TargetUnitId.Should().Be(unitId);
        doc.TargetSubunitId.Should().NotBeNull();
    }

    [Fact]
    public void ApplySnapshot_sets_rows_and_bumps_version_once()
    {
        var doc = NewDoc();
        doc.ApplySnapshot(
            new[] { new CompetitorResult(EntryId.New(), 1) { Wlt = Wlt.W }, new CompetitorResult(EntryId.New(), 2) { Wlt = Wlt.L } },
            Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), Schema);
        doc.Competitors.Should().HaveCount(2);
        doc.Version.Should().Be(2);
    }

    [Fact]
    public void ApplySnapshot_rejects_duplicate_sort_order_atomically()
    {
        var doc = NewDoc();
        FluentActions.Invoking(() => doc.ApplySnapshot(
                new[] { new CompetitorResult(EntryId.New(), 1), new CompetitorResult(EntryId.New(), 1) },
                Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), Schema))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-6");
        doc.Competitors.Should().BeEmpty();
        doc.Version.Should().Be(1);
    }

    [Fact]
    public void ApplySnapshot_rejects_when_schema_validation_fails_and_restores_state()
    {
        var doc = NewDoc();
        var rejecting = new RejectingSchema();
        FluentActions.Invoking(() => doc.ApplySnapshot(
                new[] { new CompetitorResult(EntryId.New(), 1) },
                Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), rejecting))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-8");
        doc.Competitors.Should().BeEmpty();
        doc.Version.Should().Be(1);
    }

    [Fact]
    public void TransitionTo_is_rejected_when_status_outside_schema_set()
    {
        var schema = new FixedStatusSchema(new HashSet<ResultStatus> { ResultStatus.StartList, ResultStatus.Live });
        FluentActions.Invoking(() => NewDoc().TransitionTo(ResultStatus.Official, schema))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-4");
    }

    [Fact]
    public void ApplyRollup_is_atomic_when_suggested_transition_is_illegal()
    {
        var schema = new FixedStatusSchema(new HashSet<ResultStatus> { ResultStatus.StartList }); // Official not allowed
        var doc = NewDoc();
        var rollup = new ResultRollup(
            new[] { new CompetitorResult(EntryId.New(), 1) },
            Array.Empty<ResultExtension>(),
            ResultStatus.Official);
        FluentActions.Invoking(() => doc.ApplyRollup(rollup, schema))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-4");
        doc.Competitors.Should().BeEmpty();   // nothing applied
        doc.Version.Should().Be(1);
    }

    [Fact]
    public void TransitionTo_rejects_an_illegal_intra_set_transition()
    {
        var schema = new DefaultResultSchema();   // full status set, coarse transition rules
        var doc = NewDoc();
        doc.TransitionTo(ResultStatus.Official, schema);   // allowed
        FluentActions.Invoking(() => doc.TransitionTo(ResultStatus.Live, schema))   // Official is terminal
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-4");
    }

    [Fact]
    public void ApplySnapshot_propagates_the_schema_error_code_when_present()
    {
        var doc = NewDoc();
        var rejecting = new CodedRejectingSchema();
        FluentActions.Invoking(() => doc.ApplySnapshot(
                new[] { new CompetitorResult(EntryId.New(), 1) },
                Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), rejecting))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-5");
        doc.Competitors.Should().BeEmpty();
        doc.Version.Should().Be(1);
    }

    private sealed class RejectingSchema : DefaultResultSchema
    {
        public override Result Validate(UnitResultDocument document) => Result.Fail("nope");
    }

    private sealed class CodedRejectingSchema : DefaultResultSchema
    {
        public override Result Validate(UnitResultDocument document) => Result.Fail("I-RES-5", "nope");
    }

    private sealed class FixedStatusSchema(IReadOnlySet<ResultStatus> statuses) : DefaultResultSchema
    {
        public override IReadOnlySet<ResultStatus> StatusesFor(EventTypeCode type) => statuses;
    }
}
