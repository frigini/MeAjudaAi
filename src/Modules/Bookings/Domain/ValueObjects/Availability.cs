using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Bookings.Domain.ValueObjects;

public sealed class Availability : ValueObject
{
    private readonly List<TimeSlot> _slots = [];
    public DayOfWeek DayOfWeek { get; }
    public IReadOnlyList<TimeSlot> Slots => _slots.AsReadOnly();

    private Availability() { } // Required by EF Core

    private Availability(DayOfWeek dayOfWeek, IEnumerable<TimeSlot> slots)
    {
        DayOfWeek = dayOfWeek;
        _slots.AddRange(slots.OrderBy(s => s.Start));

        ValidateNoOverlaps();
    }

    public static Availability Create(DayOfWeek dayOfWeek, IEnumerable<TimeSlot> slots) 
        => new(dayOfWeek, slots);

    private void ValidateNoOverlaps()
    {
        for (int i = 0; i < _slots.Count - 1; i++)
        {
            if (_slots[i].Overlaps(_slots[i + 1]))
            {
                throw new InvalidOperationException($"Availability slots for {DayOfWeek} cannot overlap.");
            }
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DayOfWeek;
        foreach (var slot in _slots)
        {
            yield return slot;
        }
    }
}
