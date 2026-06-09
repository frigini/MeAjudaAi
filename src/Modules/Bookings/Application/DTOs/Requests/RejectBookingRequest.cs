using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Application.DTOs.Requests;

/// <summary>
/// Representa a requisição de rejeição de um agendamento.
/// </summary>
[ExcludeFromCodeCoverage]
public record RejectBookingRequest(string Reason);
