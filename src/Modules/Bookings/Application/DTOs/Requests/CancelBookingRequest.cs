using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Application.DTOs.Requests;

/// <summary>
/// Representa a requisição de cancelamento de uma reserva.
/// </summary>
[ExcludeFromCodeCoverage]
public record CancelBookingRequest(
    /// <summary>
    /// Justificativa ou razão do cancelamento.
    /// </summary>
    string Reason);
