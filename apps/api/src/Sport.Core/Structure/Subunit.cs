using Sport.Core.Shared;

namespace Sport.Core.Structure;

public sealed class Subunit
{
    public SubunitId Id { get; }
    public UnitId UnitId { get; }
    public SubunitCode Code { get; }
    public Rsc Rsc { get; }

    private Subunit(SubunitId id, UnitId unitId, SubunitCode code, Rsc rsc)
    {
        Id = id; UnitId = unitId; Code = code; Rsc = rsc;
    }

    public static Subunit Create(SubunitId id, UnitId unitId, SubunitCode code, Rsc parentUnitRsc)
    {
        var s = parentUnitRsc.Value;
        if (s[32] != '0' || s[33] != '0')
            throw new DomainException("I-STR-7", "Parent Unit RSC must end with '00' to host subunits.");

        var composed = string.Concat(s.AsSpan(0, 32), code.Value);
        return new Subunit(id, unitId, code, Rsc.From(composed));
    }
}
