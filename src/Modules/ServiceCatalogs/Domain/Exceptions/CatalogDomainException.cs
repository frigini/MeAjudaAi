using MeAjudaAi.Shared.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma regra de domínio é violada no módulo ServiceCatalogs.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class CatalogDomainException : DomainException
{
    public CatalogDomainException(string message) : base(message) { }

    public CatalogDomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
