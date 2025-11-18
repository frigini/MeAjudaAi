namespace MeAjudaAi.Modules.Catalogs.Domain.Exceptions;

/// <summary>
/// Exception thrown when a domain rule is violated in the Catalogs module.
/// </summary>
public sealed class CatalogDomainException : Exception
{
    public CatalogDomainException(string message) : base(message) { }

    public CatalogDomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
