using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Bookings.Domain.Entities;

public sealed class ProviderSchedule : BaseEntity
{
    private readonly List<Availability> _availabilities = [];
    public Guid ProviderId { get; private set; }
    public IReadOnlyList<Availability> Availabilities => _availabilities.AsReadOnly();

    private ProviderSchedule() { } // Required by EF Core

    private ProviderSchedule(Guid providerId)
    {
        ProviderId = providerId;
    }

    public static ProviderSchedule Create(Guid providerId) => new(providerId);

    public void SetAvailability(Availability availability)
    {
        var existing = _availabilities.FirstOrDefault(a => a.DayOfWeek == availability.DayOfWeek);
        if (existing != null)
        {
            _availabilities.Remove(existing);
        }

        _availabilities.Add(availability);
        MarkAsUpdated();
    }

    public bool IsAvailable(DateTime dateTime, TimeSpan duration)
    {
        var dayAvailability = _availabilities.FirstOrDefault(a => a.DayOfWeek == dateTime.DayOfWeek);
        if (dayAvailability == null) return false;

        var requestStart = dateTime;
        var requestEnd = dateTime.Add(duration);

        // Verifica se o intervalo solicitado está dentro de algum dos slots permitidos do dia
        // NOTA: Para simplificar, assumimos que o agendamento não vira o dia.
        return dayAvailability.Slots.Any(slot => 
            requestStart.TimeOfDay >= slot.Start.TimeOfDay && 
            requestEnd.TimeOfDay <= slot.End.TimeOfDay);
    }
}
