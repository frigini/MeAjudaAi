namespace MeAjudaAi.Contracts.Modules.Bookings.DTOs;

public record SetProviderScheduleRequestDto(
    Guid ProviderId,
    IEnumerable<AvailabilityDto> Availabilities);
