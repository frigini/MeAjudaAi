namespace MeAjudaAi.Modules.Locations.Domain.Exceptions;

/// <summary>
/// Exceção base para requisições inválidas (400 Bad Request).
/// </summary>
public abstract class BadRequestException(string message) : Exception(message)
{
}
