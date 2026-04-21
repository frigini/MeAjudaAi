using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Bookings.Domain.ValueObjects;

/// <summary>
/// Representa um intervalo de tempo (hora início e hora fim) sem data associada.
/// </summary>
public sealed class TimeSlot : ValueObject
{
    public TimeOnly Start { get; }
    public TimeOnly End { get; }

    private TimeSlot() { } // Required by EF Core

    private TimeSlot(TimeOnly start, TimeOnly end)
    {
        if (start >= end)
        {
            throw new ArgumentException("Start time must be before end time.");
        }

        Start = start;
        End = end;
    }

    /// <summary>
    /// Cria um TimeSlot a partir de TimeOnly.
    /// </summary>
    public static TimeSlot Create(TimeOnly start, TimeOnly end) => new(start, end);

    /// <summary>
    /// Cria um TimeSlot a partir de DateTime (ignora a data).
    /// </summary>
    public static TimeSlot FromDateTime(DateTime start, DateTime end) 
        => new(TimeOnly.FromDateTime(start), TimeOnly.FromDateTime(end));

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
