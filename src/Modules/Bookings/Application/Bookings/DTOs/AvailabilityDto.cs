namespace MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;

/// <summary>
/// DTO para representação de um slot de tempo. 
/// Usa DateTime para facilitar a serialização JSON no frontend, 
/// mas apenas a parte da hora é relevante para a agenda semanal.
/// </summary>
public record TimeSlotDto(DateTime Start, DateTime End);

public record AvailabilityDto(DayOfWeek DayOfWeek, IEnumerable<TimeSlotDto> Slots);
