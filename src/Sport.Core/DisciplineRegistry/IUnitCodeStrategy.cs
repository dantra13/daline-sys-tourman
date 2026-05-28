using Sport.Core.Structure;

namespace Sport.Core.DisciplineRegistry;

public interface IUnitCodeStrategy
{
    UnitCode NextUnitCode(IEnumerable<UnitCode> existing);
    bool IsValid(UnitCode code);
}
