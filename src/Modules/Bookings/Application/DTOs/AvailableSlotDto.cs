namespace MeAjudaAi.Modules.Bookings.Application.DTOs;

/// <summary>
/// Representa um intervalo de horário disponível.
/// </summary>
public record AvailableSlotDto(DateTimeOffset Start, DateTimeOffset End);
