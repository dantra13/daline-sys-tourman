using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD;

// Produces atomic ('--'-trailing) unit codes for individual contests. Team-match parent codes
// (ending '00') are supplied explicitly when assembling a team match via Event.AddTeamMatchUnit.
public sealed class JudUnitCodeStrategy : IUnitCodeStrategy
{
    public UnitCode NextUnitCode(IEnumerable<UnitCode> existing)
    {
        var max = existing
            .Select(u => int.TryParse(u.Value.AsSpan(0, 6), out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return UnitCode.From($"{(max + 100).ToString("D6")}--");
    }

    public bool IsValid(UnitCode code) => true;
}
