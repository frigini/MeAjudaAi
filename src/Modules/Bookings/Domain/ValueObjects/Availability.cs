using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Bookings.Domain.ValueObjects;

/// <summary>
/// Representa a disponibilidade de horários para um dia da semana específico.
/// </summary>
public sealed class Availability : ValueObject
{
    private readonly List<TimeSlot> _slots = [];
    public DayOfWeek DayOfWeek { get; }
    public IReadOnlyList<TimeSlot> Slots => _slots.AsReadOnly();

    private Availability() { } // Necessário para o EF Core

    private Availability(DayOfWeek dayOfWeek, IEnumerable<TimeSlot> slots)
    {
        ArgumentNullException.ThrowIfNull(slots);

        DayOfWeek = dayOfWeek;
        _slots.AddRange(slots.OrderBy(s => s.Start));

        ValidateNoOverlaps();
    }

    /// <summary>
    /// Cria uma nova disponibilidade garantindo que não haja sobreposição entre os horários.
    /// NOTA: Slots adjacentes (ex: 09:00-10:00 e 10:00-11:00) são permitidos (limites exclusivos no Overlaps).
    /// </summary>
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
