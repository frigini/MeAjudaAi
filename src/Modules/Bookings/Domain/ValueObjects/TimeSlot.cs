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
    /// <exception cref="ArgumentException">Lançada se as datas ou Kinds de DateTime e EndTime forem diferentes.</exception>
    public static TimeSlot FromDateTime(DateTime start, DateTime end)
    {
        if (start.Date != end.Date || start.Kind != end.Kind)
        {
            throw new ArgumentException($"Start and End must have the same Date and Kind. Start: {start}, End: {end}");
        }

        return new(TimeOnly.FromDateTime(start), TimeOnly.FromDateTime(end));
    }

    public bool Overlaps(TimeSlot other)
    {
        return Start < other.End && other.Start < End;
    }

    /// <summary>
    /// Subtrai uma lista de intervalos ocupados deste TimeSlot, retornando os intervalos livres resultantes.
    /// </summary>
    public IReadOnlyList<TimeSlot> Subtract(IEnumerable<TimeSlot> occupiedSlots)
    {
        var freeSlots = new List<TimeSlot> { this };

        foreach (var occupied in occupiedSlots)
        {
            var nextFreeSlots = new List<TimeSlot>();
            foreach (var free in freeSlots)
            {
                if (!free.Overlaps(occupied))
                {
                    nextFreeSlots.Add(free);
                    continue;
                }

                if (free.Start < occupied.Start)
                {
                    nextFreeSlots.Add(Create(free.Start, occupied.Start));
                }

                if (free.End > occupied.End)
                {
                    nextFreeSlots.Add(Create(occupied.End, free.End));
                }
            }
            freeSlots = nextFreeSlots;
        }

        return freeSlots;
    }

    public TimeSpan Duration => End - Start;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}
