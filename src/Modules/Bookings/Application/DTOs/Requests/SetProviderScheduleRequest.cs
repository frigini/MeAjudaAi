using MeAjudaAi.Modules.Bookings.Application.DTOs;

namespace MeAjudaAi.Modules.Bookings.Application.DTOs.Requests;

public record SetProviderScheduleRequest(
    Guid ProviderId,
    IEnumerable<AvailabilityDto> Availabilities);
