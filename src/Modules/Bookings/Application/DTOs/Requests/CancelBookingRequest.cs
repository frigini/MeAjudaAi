using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Application.DTOs.Requests;

[ExcludeFromCodeCoverage]
public record CancelBookingRequest(string Reason);
