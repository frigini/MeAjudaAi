namespace MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;

/// <summary>
/// DTO para representação de um slot de tempo.
/// Usa TimeOnly para representar apenas a parte da hora, que é o relevante.
/// </summary>
public record TimeSlotDto(TimeOnly Start, TimeOnly End);

public record AvailabilityDto(DayOfWeek DayOfWeek, IReadOnlyList<TimeSlotDto> Slots);
