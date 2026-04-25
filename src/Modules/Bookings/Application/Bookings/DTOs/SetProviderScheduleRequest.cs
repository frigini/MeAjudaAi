namespace MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;

public record SetProviderScheduleRequest(
    Guid ProviderId,
    IEnumerable<ProviderScheduleDto> Availabilities);
