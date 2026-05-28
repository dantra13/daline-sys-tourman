using Sport.Core.DisciplineRegistry;
using Vogen;

namespace Sport.Core.Structure;

// Slot offsets within the 34-char RSC layout:
//   [0..3)   Discipline (3)
//   [3..4)   Gender     (1)
//   [4..12)  EventType  (8)
//   [12..22) EventMod   (10)
//   [22..26) Phase      (4)
//   [26..32) Unit body  (6)
//   [32..34) Subunit / unit trailing (2)
[ValueObject<string>]
public readonly partial struct Rsc
{
    public const int Length = 34;
    public const char Filler = '-';

    private static Validation Validate(string value)
    {
        if (value is null) return Validation.Invalid("RSC is required.");
        if (value.Length != Length) return Validation.Invalid($"RSC must be exactly {Length} characters.");
        foreach (var c in value)
        {
            var ok = c is >= 'A' and <= 'Z'
                  || c is >= '0' and <= '9'
                  || c == '.'
                  || c == '-';
            if (!ok) return Validation.Invalid("RSC may only contain A-Z, 0-9, '.' and '-'.");
        }
        return Validation.Ok;
    }

    public RscLevel Level => ComputeLevel(Value);

    public static Rsc Compose(
        DisciplineCode discipline,
        GenderCode gender,
        EventTypeCode eventType,
        EventModifierCode? modifier,
        PhaseCode? phase,
        UnitCode? unit,
        SubunitCode? subunit)
    {
        Span<char> buf = stackalloc char[Length];
        buf.Fill(Filler);

        discipline.Value.AsSpan().CopyTo(buf[..3]);
        buf[3] = gender.ToRscChar();
        PadInto(eventType.Value, buf[4..12]);
        if (modifier is { } m) PadInto(m.Value, buf[12..22]);

        if (phase is { } p) PadInto(p.Value, buf[22..26]);
        if (unit is { } u) PadInto(u.Value, buf[26..34]);

        if (subunit is { } s)
        {
            var sub = s.Value;
            if (sub.Length != 2)
                throw new ArgumentException("SubunitCode must be exactly 2 characters.", nameof(subunit));
            buf[32] = sub[0];
            buf[33] = sub[1];
        }

        return From(new string(buf));
    }

    private static void PadInto(string source, Span<char> dest)
    {
        if (source.Length > dest.Length)
            throw new ArgumentException($"Value '{source}' too long for {dest.Length}-char slot.");
        source.AsSpan().CopyTo(dest[..source.Length]);
    }

    private static RscLevel ComputeLevel(string value)
    {
        static bool SlotFilled(string s, Range r)
        {
            foreach (var c in s.AsSpan()[r])
                if (c != Filler) return true;
            return false;
        }

        var hasEvent   = SlotFilled(value, 4..22);
        var hasPhase   = SlotFilled(value, 22..26);
        var hasUnit    = SlotFilled(value, 26..32);
        var hasSubunit = SlotFilled(value, 32..34);

        if (hasSubunit) return RscLevel.Subunit;
        if (hasUnit) return RscLevel.Unit;
        if (hasPhase) return RscLevel.Phase;
        if (hasEvent) return RscLevel.Event;
        return RscLevel.Discipline;
    }
}
