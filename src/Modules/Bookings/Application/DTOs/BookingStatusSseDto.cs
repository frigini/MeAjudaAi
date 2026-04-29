namespace MeAjudaAi.Modules.Bookings.Application.DTOs;

/// <summary>
/// DTO para eventos SSE de alteração de status de reserva.
/// </summary>
public record BookingStatusSseDto(
    Guid BookingId,
    string Status,
    DateTime UpdatedAt,
    string? Message = null);
