using MeAjudaAi.Shared.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Domain.Exceptions;

[ExcludeFromCodeCoverage]
public class InvalidBookingStateException(string message) : DomainException(message);
