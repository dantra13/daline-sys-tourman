using Sport.Core.DisciplineRegistry;
using Sport.Infrastructure.Tests.TestHelpers;

namespace Sport.Infrastructure.Tests.Persistence;

internal static class InfraTestRegistry
{
    public static IDisciplineRegistry WithFblM()
    {
        var reg = new FakeRegistry();
        var fbl = DisciplineCode.From("FBL");
        reg.SupportedCodes.Add(fbl);
        reg.GendersByCode[fbl] = new HashSet<GenderCode> { GenderCode.M };
        return reg;
    }
}
