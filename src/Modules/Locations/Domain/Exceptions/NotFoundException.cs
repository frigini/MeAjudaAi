namespace MeAjudaAi.Modules.Locations.Domain.Exceptions;

/// <summary>
/// Exceção base para recursos não encontrados (404 Not Found).
/// </summary>
public abstract class NotFoundException(string message) : Exception(message)
{
}
