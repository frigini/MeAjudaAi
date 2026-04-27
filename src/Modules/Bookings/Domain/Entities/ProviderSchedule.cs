using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Bookings.Domain.Entities;

public sealed class ProviderSchedule : BaseEntity
{
    private readonly List<Availability> _availabilities = [];
    public Guid ProviderId { get; private set; }
    public string TimeZoneId { get; private set; } = "America/Sao_Paulo"; // Padrão Brasília (IANA)
    public IReadOnlyList<Availability> Availabilities => _availabilities.AsReadOnly();

    private ProviderSchedule() { } // Required by EF Core

    private ProviderSchedule(Guid providerId, string? timeZoneId = null)
    {
        ProviderId = providerId;
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            ValidateTimeZoneId(timeZoneId);
            TimeZoneId = timeZoneId;
        }
    }

    public static ProviderSchedule Create(Guid providerId, string? timeZoneId = null) 
        => new(providerId, timeZoneId);

    public void UpdateTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId)) throw new ArgumentException("TimeZoneId não pode estar vazio", nameof(timeZoneId));
        
        ValidateTimeZoneId(timeZoneId);
        TimeZoneId = timeZoneId;
        MarkAsUpdated();
    }

    private void ValidateTimeZoneId(string timeZoneId)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ArgumentException($"TimeZoneId inválido: {timeZoneId}", nameof(timeZoneId));
        }
        catch (InvalidTimeZoneException)
        {
            throw new ArgumentException($"TimeZoneId inválido: {timeZoneId}", nameof(timeZoneId));
        }
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

    public bool IsAvailable(DateTime localDateTime, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero) return false;
        
        var endDate = localDateTime.Add(duration);
        var requestStart = TimeOnly.FromDateTime(localDateTime);
        var requestEnd = TimeOnly.FromDateTime(endDate);

        if (endDate.Date != localDateTime.Date) return false;

        var dayAvailability = _availabilities.FirstOrDefault(a => a.DayOfWeek == localDateTime.DayOfWeek);
        if (dayAvailability == null) return false;

        return dayAvailability.Slots.Any(slot => 
            requestStart >= slot.Start && 
            requestEnd <= slot.End);
    }
}
