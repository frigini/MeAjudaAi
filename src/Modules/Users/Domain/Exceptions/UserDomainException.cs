using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Modules.Users.Domain.Exceptions;

/// <summary>
/// Exceção específica do domínio de usuários para violações de regras de negócio.
/// </summary>
public class UserDomainException : DomainException
{
    /// <summary>
    /// Inicializa uma nova instância de UserDomainException.
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    public UserDomainException(string message) : base(message)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância de UserDomainException com exceção interna.
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <param name="innerException">Exceção que causou este erro</param>
    public UserDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Cria uma exceção para erro de validação de campo.
    /// </summary>
    /// <param name="fieldName">Nome do campo inválido</param>
    /// <param name="invalidValue">Valor inválido fornecido</param>
    /// <param name="reason">Razão específica da invalidez</param>
    /// <returns>Instância configurada de UserDomainException</returns>
    public static UserDomainException ForValidationError(string fieldName, object? invalidValue, string reason)
    {
        return new UserDomainException($"Validation failed for field '{fieldName}': {reason}");
    }

    /// <summary>
    /// Cria uma exceção para operação inválida.
    /// </summary>
    /// <param name="operation">Nome da operação que falhou</param>
    /// <param name="currentState">Estado atual que impede a operação</param>
    /// <returns>Instância configurada de UserDomainException</returns>
    public static UserDomainException ForInvalidOperation(string operation, string currentState)
    {
        return new UserDomainException($"Cannot perform operation '{operation}' in current state: {currentState}");
    }

    /// <summary>
    /// Cria uma exceção para formato inválido.
    /// </summary>
    /// <param name="fieldName">Nome do campo com formato inválido</param>
    /// <param name="invalidValue">Valor com formato inválido</param>
    /// <param name="expectedFormat">Formato esperado</param>
    /// <returns>Instância configurada de UserDomainException</returns>
    public static UserDomainException ForInvalidFormat(string fieldName, object? invalidValue, string expectedFormat)
    {
        return new UserDomainException($"Invalid format for field '{fieldName}'. Expected: {expectedFormat}");
    }
}