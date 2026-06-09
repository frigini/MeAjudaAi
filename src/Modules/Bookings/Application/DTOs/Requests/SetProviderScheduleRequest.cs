using MeAjudaAi.Modules.Bookings.Application.DTOs;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Application.DTOs.Requests;

[ExcludeFromCodeCoverage]
public record SetProviderScheduleRequest(
    Guid ProviderId,
    IEnumerable<AvailabilityDto> Availabilities);
