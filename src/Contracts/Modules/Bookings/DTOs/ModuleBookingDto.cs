using MeAjudaAi.Contracts.Modules.Bookings.Enums;

namespace MeAjudaAi.Contracts.Modules.Bookings.DTOs;

/// <summary>
/// DTO de agendamento para uso entre módulos.
/// </summary>
public record ModuleBookingDto(
    Guid Id,
    Guid ProviderId,
    Guid ClientId,
    Guid ServiceId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    EBookingStatus Status,
    string? RejectionReason = null,
    string? CancellationReason = null);