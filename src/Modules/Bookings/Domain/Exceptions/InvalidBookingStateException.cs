using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Modules.Bookings.Domain.Exceptions;

public class InvalidBookingStateException(string message) : DomainException(message);
