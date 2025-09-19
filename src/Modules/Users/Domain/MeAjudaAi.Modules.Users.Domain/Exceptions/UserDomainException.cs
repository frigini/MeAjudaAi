using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Modules.Users.Domain.Exceptions;

/// <summary>
/// Exceção específica do domínio de usuários para violações de regras de negócio.
/// </summary>
/// <remarks>
/// Esta exceção é lançada quando operações no domínio de usuários violam
/// regras de negócio específicas, como:
/// - Validações de dados obrigatórios
/// - Regras de formato (email, username)
/// - Restrições de estado (usuário deletado)
/// - Limites de tamanho de campos
/// - Regras de unicidade (quando aplicável)
/// 
/// Herda de DomainException que implementa o padrão de exceções de domínio.
/// </remarks>
public class UserDomainException : DomainException
{
    /// <summary>
    /// Tipos específicos de erros do domínio de usuários.
    /// </summary>
    public enum UserErrorType
    {
        /// <summary>Erro de validação de dados de entrada</summary>
        ValidationError,
        /// <summary>Operação não permitida no estado atual</summary>
        InvalidOperation,
        /// <summary>Formato inválido de dados</summary>
        InvalidFormat,
        /// <summary>Violação de regra de negócio</summary>
        BusinessRuleViolation,
        /// <summary>Estado inconsistente da entidade</summary>
        InvalidState
    }

    /// <summary>
    /// Tipo específico do erro de usuário.
    /// </summary>
    public UserErrorType ErrorType { get; }

    /// <summary>
    /// Campo específico relacionado ao erro, se aplicável.
    /// </summary>
    public string? FieldName { get; }

    /// <summary>
    /// Valor que causou o erro, se aplicável.
    /// </summary>
    public object? InvalidValue { get; }

    /// <summary>
    /// Inicializa uma nova instância de UserDomainException.
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    public UserDomainException(string message) : base(message)
    {
        ErrorType = UserErrorType.BusinessRuleViolation;
    }

    /// <summary>
    /// Inicializa uma nova instância de UserDomainException com exceção interna.
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <param name="innerException">Exceção que causou este erro</param>
    public UserDomainException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorType = UserErrorType.BusinessRuleViolation;
    }

    /// <summary>
    /// Inicializa uma nova instância de UserDomainException com parâmetros formatados.
    /// </summary>
    /// <param name="message">Mensagem com placeholders para formatação</param>
    /// <param name="args">Argumentos para formatação da mensagem</param>
    public UserDomainException(string message, params object[] args) : base(string.Format(message, args))
    {
        ErrorType = UserErrorType.BusinessRuleViolation;
    }

    /// <summary>
    /// Inicializa uma nova instância de UserDomainException com tipo específico.
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <param name="errorType">Tipo específico do erro</param>
    /// <param name="fieldName">Nome do campo relacionado ao erro</param>
    /// <param name="invalidValue">Valor que causou o erro</param>
    public UserDomainException(
        string message, 
        UserErrorType errorType, 
        string? fieldName = null, 
        object? invalidValue = null) : base(message)
    {
        ErrorType = errorType;
        FieldName = fieldName;
        InvalidValue = invalidValue;
    }

    /// <summary>
    /// Inicializa uma nova instância de UserDomainException completa.
    /// </summary>
    /// <param name="message">Mensagem descritiva do erro</param>
    /// <param name="innerException">Exceção que causou este erro</param>
    /// <param name="errorType">Tipo específico do erro</param>
    /// <param name="fieldName">Nome do campo relacionado ao erro</param>
    /// <param name="invalidValue">Valor que causou o erro</param>
    public UserDomainException(
        string message, 
        Exception innerException, 
        UserErrorType errorType, 
        string? fieldName = null, 
        object? invalidValue = null) : base(message, innerException)
    {
        ErrorType = errorType;
        FieldName = fieldName;
        InvalidValue = invalidValue;
    }

    /// <summary>
    /// Inicializa uma nova instância com formatação e exceção interna.
    /// </summary>
    /// <param name="message">Mensagem com placeholders para formatação</param>
    /// <param name="innerException">Exceção que causou este erro</param>
    /// <param name="args">Argumentos para formatação da mensagem</param>
    public UserDomainException(string message, Exception innerException, params object[] args) 
        : base(string.Format(message, args), innerException)
    {
        ErrorType = UserErrorType.BusinessRuleViolation;
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
        return new UserDomainException(
            $"Validation failed for field '{fieldName}': {reason}",
            UserErrorType.ValidationError,
            fieldName,
            invalidValue);
    }

    /// <summary>
    /// Cria uma exceção para operação inválida.
    /// </summary>
    /// <param name="operation">Nome da operação que falhou</param>
    /// <param name="currentState">Estado atual que impede a operação</param>
    /// <returns>Instância configurada de UserDomainException</returns>
    public static UserDomainException ForInvalidOperation(string operation, string currentState)
    {
        return new UserDomainException(
            $"Cannot perform operation '{operation}' in current state: {currentState}",
            UserErrorType.InvalidOperation);
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
        return new UserDomainException(
            $"Invalid format for field '{fieldName}'. Expected: {expectedFormat}",
            UserErrorType.InvalidFormat,
            fieldName,
            invalidValue);
    }

    /// <summary>
    /// Retorna uma representação textual detalhada da exceção.
    /// </summary>
    /// <returns>String formatada com detalhes da exceção</returns>
    public override string ToString()
    {
        var details = new List<string> { base.ToString() };
        
        details.Add($"ErrorType: {ErrorType}");
        
        if (!string.IsNullOrEmpty(FieldName))
            details.Add($"FieldName: {FieldName}");
            
        if (InvalidValue != null)
            details.Add($"InvalidValue: {InvalidValue}");
            
        return string.Join(Environment.NewLine, details);
    }
}