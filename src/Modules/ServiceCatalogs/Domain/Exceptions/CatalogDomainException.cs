namespace MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma regra de domínio é violada no módulo ServiceCatalogs.
/// </summary>
public sealed class CatalogDomainException : Exception
{
    public CatalogDomainException() { }

    public CatalogDomainException(string message) : base(message) { }

    public CatalogDomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
