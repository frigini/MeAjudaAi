using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Domain.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class UpstreamProviderException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
