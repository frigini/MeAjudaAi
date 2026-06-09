using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Application.DTOs.Requests;

/// <summary>
/// Representa a requisição de cancelamento de uma reserva.
/// </summary>
/// <param name="Reason">Justificativa ou razão do cancelamento.</param>
[ExcludeFromCodeCoverage]
public record CancelBookingRequest(string Reason);
