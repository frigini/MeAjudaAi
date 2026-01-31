using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Modules.Locations.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando um argumento inválido é fornecido para operações do domínio de Locations.
/// </summary>
public class InvalidLocationArgumentException : DomainException
{
    public InvalidLocationArgumentException(string message) : base(message)
    {
    }

    public InvalidLocationArgumentException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
