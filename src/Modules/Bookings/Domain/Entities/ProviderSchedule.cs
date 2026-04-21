using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Bookings.Domain.Entities;

public sealed class ProviderSchedule : BaseEntity
{
    private readonly List<Availability> _availabilities = [];
    public Guid ProviderId { get; private set; }
    public string TimeZoneId { get; private set; } = "E. South America Standard Time"; // Padrão Brasília
    public IReadOnlyList<Availability> Availabilities => _availabilities.AsReadOnly();

    private ProviderSchedule() { } // Required by EF Core

    private ProviderSchedule(Guid providerId, string? timeZoneId = null)
    {
        ProviderId = providerId;
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            TimeZoneId = timeZoneId;
        }
    }

    public static ProviderSchedule Create(Guid providerId, string? timeZoneId = null) 
        => new(providerId, timeZoneId);

    public void UpdateTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId)) throw new ArgumentException("TimeZoneId cannot be empty");
        TimeZoneId = timeZoneId;
        MarkAsUpdated();
    }

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

    public void ClearAvailabilities()
    {
        _availabilities.Clear();
        MarkAsUpdated();
    }

    public bool IsAvailable(DateTime dateTime, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero) return false;
        
        var requestStart = dateTime;
        var requestEnd = dateTime.Add(duration);

        // Rejeita intervalos que cruzam a meia-noite
        if (requestEnd.Date != requestStart.Date) return false;

        var dayAvailability = _availabilities.FirstOrDefault(a => a.DayOfWeek == dateTime.DayOfWeek);
        if (dayAvailability == null) return false;

        // Verifica se o intervalo solicitado está dentro de algum dos slots permitidos do dia
        return dayAvailability.Slots.Any(slot => 
            requestStart.TimeOfDay >= slot.Start.TimeOfDay && 
            requestEnd.TimeOfDay <= slot.End.TimeOfDay);
    }
}
