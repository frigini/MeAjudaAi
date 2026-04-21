using System.Diagnostics.CodeAnalysis;
namespace MeAjudaAi.Shared.Exceptions;

[ExcludeFromCodeCoverage]

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base("Access to this resource is forbidden.")
    {
    }

    public ForbiddenAccessException(string message) : base(message)
    {
    }

    public ForbiddenAccessException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
