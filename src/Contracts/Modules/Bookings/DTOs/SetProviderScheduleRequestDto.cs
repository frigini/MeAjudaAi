namespace MeAjudaAi.Contracts.Modules.Bookings.DTOs;

public sealed record SetProviderScheduleRequestDto(
    Guid ProviderId,
    IEnumerable<AvailabilityDto> Availabilities);
