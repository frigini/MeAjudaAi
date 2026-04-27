namespace MeAjudaAi.Modules.Bookings.Domain.Exceptions;

public class InvalidBookingStateException(string message) : InvalidOperationException(message);
