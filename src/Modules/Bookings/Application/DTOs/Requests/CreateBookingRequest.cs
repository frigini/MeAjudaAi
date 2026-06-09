using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Application.DTOs.Requests;

/// <summary>
/// Representa a requisição para criação de um novo agendamento.
/// </summary>
[ExcludeFromCodeCoverage]
public record CreateBookingRequest(
    Guid ProviderId,
    Guid ServiceId,
    DateTimeOffset Start,
    DateTimeOffset End);
