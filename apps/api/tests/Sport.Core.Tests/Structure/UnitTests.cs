using FluentAssertions;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Tests.Structure;

public class UnitTests
{
    private static readonly Rsc PhaseRsc = Rsc.From("FBLMTEAM11------------QFNL--------");

    [Fact]
    public void Create_atomic_unit_composes_rsc_with_unit_code()
    {
        var unit = Unit.CreateAtomic(UnitId.New(), PhaseId.New(), UnitCode.From("000100--"), PhaseRsc, null);
        unit.Rsc.Value.Should().Be("FBLMTEAM11------------QFNL000100--");
        unit.Subunits.Should().BeEmpty();
    }

    [Fact]
    public void Create_parent_unit_requires_code_ending_in_00()
    {
        var act = () => Unit.CreateParentForSubunits(UnitId.New(), PhaseId.New(), UnitCode.From("000100--"), PhaseRsc, null);
        act.Should().Throw<DomainException>().WithMessage("*end with '00'*");
    }

    [Fact]
    public void LinkDisciplineRef_sets_value_once()
    {
        var unit = Unit.CreateAtomic(UnitId.New(), PhaseId.New(), UnitCode.From("000100--"), PhaseRsc, null);
        var refId = Guid.NewGuid();
        unit.LinkDisciplineRef(refId);
        unit.DisciplineUnitRef.Should().Be(refId);

        var act = () => unit.LinkDisciplineRef(Guid.NewGuid());
        act.Should().Throw<DomainException>().WithMessage("*already linked*");
    }

    [Fact]
    public void CreateAtomic_rejects_non_filler_trailing()
    {
        var act = () => Unit.CreateAtomic(
            UnitId.New(), PhaseId.New(), UnitCode.From("00010000"), PhaseRsc, null);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-STR-14");
    }
}
