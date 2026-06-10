using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Modules.Bookings.Application.DTOs;

namespace MeAjudaAi.Modules.Bookings.Application.DTOs.Requests;

/// <summary>
/// Representa a requisição para definir a grade de disponibilidade do prestador.
/// </summary>
[ExcludeFromCodeCoverage]
public record SetProviderScheduleRequest(
    Guid ProviderId,
    IEnumerable<AvailabilityDto> Availabilities);
