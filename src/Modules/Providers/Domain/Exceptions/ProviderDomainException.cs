using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Modules.Providers.Domain.Exceptions;

/// <summary>
/// Exceção específica do domínio de prestadores de serviços.
/// </summary>
/// <remarks>
/// Esta exceção é lançada quando regras de negócio específicas do domínio
/// de prestadores de serviços são violadas durante operações de domínio.
/// </remarks>
public class ProviderDomainException : DomainException
{
    /// <summary>
    /// Inicializa uma nova instância da exceção com uma mensagem específica.
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    public ProviderDomainException(string message) : base(message)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da exceção com uma mensagem e uma exceção interna.
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    /// <param name="innerException">Exceção interna</param>
    public ProviderDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}