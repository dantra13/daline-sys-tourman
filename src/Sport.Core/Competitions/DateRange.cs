using Sport.Core.Shared;

namespace Sport.Core.Competitions;

public readonly record struct DateRange
{
    public DateOnly Start { get; }
    public DateOnly End { get; }
    public int Days => End.DayNumber - Start.DayNumber + 1;

    private DateRange(DateOnly start, DateOnly end)
    {
        Start = start;
        End = end;
    }

    public static DateRange Create(DateOnly start, DateOnly end)
    {
        if (start > end)
            throw new DomainException("DateRange.Start must be on or before DateRange.End.");
        return new DateRange(start, end);
    }
}
