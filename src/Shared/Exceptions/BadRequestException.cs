namespace MeAjudaAi.Shared.Exceptions;

/// <summary>
/// Exceção base para requisições inválidas (400 Bad Request).
/// </summary>
public abstract class BadRequestException(string message) : Exception(message)
{
}
