using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Modules.Locations.Domain.Exceptions;

public sealed class GeocodingException : DomainException
{
    public GeocodingException(string message) : base(message)
    {
    }

    public GeocodingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}