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
            throw new DomainException(
                "I-DR-1",
                "DateRange.Start must be on or before DateRange.End.",
                new Dictionary<string, object?>
                {
                    ["start"] = start.ToString("yyyy-MM-dd"),
                    ["end"] = end.ToString("yyyy-MM-dd"),
                });
        return new DateRange(start, end);
    }
}
