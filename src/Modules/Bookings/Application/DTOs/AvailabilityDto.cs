namespace MeAjudaAi.Modules.Bookings.Application.DTOs;

/// <summary>
/// Representa a disponibilidade de um prestador em um dia da semana.
/// </summary>
public record AvailabilityDto(DayOfWeek DayOfWeek, IReadOnlyList<AvailableSlotDto> Slots);
