using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Bookings.Domain.ValueObjects;

public sealed class TimeSlot : ValueObject
{
    public DateTime Start { get; }
    public DateTime End { get; }

    private TimeSlot() { } // Required by EF Core

    private TimeSlot(DateTime start, DateTime end)
    {
        // Garante que as datas sejam UTC
        var utcStart = start.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(start, DateTimeKind.Utc) 
            : start.ToUniversalTime();

        var utcEnd = end.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(end, DateTimeKind.Utc) 
            : end.ToUniversalTime();

        if (utcStart >= utcEnd)
        {
            throw new ArgumentException("Start time must be before end time.");
        }

        Start = utcStart;
        End = utcEnd;
    }

    public static TimeSlot Create(DateTime start, DateTime end) => new(start, end);

    public bool Overlaps(TimeSlot other)
    {
        return Start < other.End && other.Start < End;
    }

    public TimeSpan Duration => End - Start;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}
