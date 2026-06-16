namespace MeAjudaAi.Contracts.Modules.Bookings.DTOs;

/// <summary>
/// Request DTO para criação de um agendamento.
/// </summary>
public sealed record CreateBookingRequestDto(
    Guid ProviderId,
    Guid ServiceId,
    DateTimeOffset Start,
    DateTimeOffset End);