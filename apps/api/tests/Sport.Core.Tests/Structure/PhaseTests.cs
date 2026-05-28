using FluentAssertions;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Tests.Structure;

public class PhaseTests
{
    private static readonly Rsc EventRsc = Rsc.From("FBLMTEAM11------------------------");

    [Fact]
    public void Create_composes_phase_rsc()
    {
        var p = Phase.Create(PhaseId.New(), EventId.New(), PhaseCode.From("QFNL"), 1, EventRsc);
        p.Rsc.Value.Should().Be("FBLMTEAM11------------QFNL--------");
    }

    [Fact]
    public void Adding_unit_with_duplicate_code_throws_I_STR_6()
    {
        var p = Phase.Create(PhaseId.New(), EventId.New(), PhaseCode.From("QFNL"), 1, EventRsc);
        p.AddUnit(Unit.CreateAtomic(UnitId.New(), p.Id, UnitCode.From("000100--"), p.Rsc, null));

        var act = () => p.AddUnit(Unit.CreateAtomic(UnitId.New(), p.Id, UnitCode.From("000100--"), p.Rsc, null));
        act.Should().Throw<DomainException>().WithMessage("*UnitCode*already*");
    }
}
