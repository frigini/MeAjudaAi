using System.Text.Json;

namespace MeAjudaAi.Shared.Messaging.DeadLetter;

/// <summary>
/// Informações sobre uma mensagem que falhou durante o processamento
/// </summary>
public sealed class FailedMessageInfo
{
    /// <summary>
    /// ID único da mensagem
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Tipo da mensagem que falhou
    /// </summary>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>
    /// Conteúdo original da mensagem (serializado)
    /// </summary>
    public string OriginalMessage { get; set; } = string.Empty;

    /// <summary>
    /// Fila/Tópico de origem
    /// </summary>
    public string SourceQueue { get; set; } = string.Empty;

    /// <summary>
    /// Data/hora da primeira tentativa
    /// </summary>
    public DateTime FirstAttemptAt { get; set; }

    /// <summary>
    /// Data/hora da última tentativa
    /// </summary>
    public DateTime LastAttemptAt { get; set; }

    /// <summary>
    /// Número de tentativas realizadas
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Razão da falha na última tentativa
    /// </summary>
    public string LastFailureReason { get; set; } = string.Empty;

    /// <summary>
    /// Stack trace da última exceção
    /// </summary>
    public string LastStackTrace { get; set; } = string.Empty;

    /// <summary>
    /// Histórico de todas as tentativas
    /// </summary>
    public List<FailureAttempt> FailureHistory { get; set; } = new();

    /// <summary>
    /// Headers/Propriedades adicionais da mensagem
    /// </summary>
    public Dictionary<string, object> MessageHeaders { get; set; } = new();

    /// <summary>
    /// Metadados do ambiente quando a falha ocorreu
    /// </summary>
    public EnvironmentMetadata Environment { get; set; } = new();
}

/// <summary>
/// Informações sobre uma tentativa de processamento que falhou
/// </summary>
public sealed class FailureAttempt
{
    /// <summary>
    /// Número da tentativa (1, 2, 3...)
    /// </summary>
    public int AttemptNumber { get; set; }

    /// <summary>
    /// Data/hora da tentativa
    /// </summary>
    public DateTime AttemptedAt { get; set; }

    /// <summary>
    /// Tipo da exceção
    /// </summary>
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem da exceção
    /// </summary>
    public string ExceptionMessage { get; set; } = string.Empty;

    /// <summary>
    /// Stack trace da exceção
    /// </summary>
    public string StackTrace { get; set; } = string.Empty;

    /// <summary>
    /// Duração do processamento até a falha
    /// </summary>
    public TimeSpan ProcessingDuration { get; set; }

    /// <summary>
    /// Handler que estava processando a mensagem
    /// </summary>
    public string HandlerType { get; set; } = string.Empty;
}

/// <summary>
/// Metadados do ambiente onde a falha ocorreu
/// </summary>
public sealed class EnvironmentMetadata
{
    /// <summary>
    /// Nome da máquina/container
    /// </summary>
    public string MachineName { get; set; } = Environment.MachineName;

    /// <summary>
    /// Nome do ambiente (Development, Production, etc.)
    /// </summary>
    public string EnvironmentName { get; set; } = string.Empty;

    /// <summary>
    /// Versão da aplicação
    /// </summary>
    public string ApplicationVersion { get; set; } = string.Empty;

    /// <summary>
    /// Data/hora em UTC quando o registro foi criado
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Nome da instância do serviço
    /// </summary>
    public string ServiceInstance { get; set; } = string.Empty;
}

/// <summary>
/// Enumeração dos tipos de falha para classificação
/// </summary>
public enum EFailureType
{
    /// <summary>
    /// Falha temporária (rede, timeout, etc.) - retry recomendado
    /// </summary>
    Transient,

    /// <summary>
    /// Falha permanente (validação, regra de negócio) - não retry
    /// </summary>
    Permanent,

    /// <summary>
    /// Falha crítica do sistema - necessita investigação
    /// </summary>
    Critical,

    /// <summary>
    /// Falha desconhecida - usar configuração padrão
    /// </summary>
    Unknown
}

/// <summary>
/// Extensões para facilitar o trabalho com FailedMessageInfo
/// </summary>
public static class FailedMessageInfoExtensions
{
    /// <summary>
    /// Serializa FailedMessageInfo para JSON
    /// </summary>
    public static string ToJson(this FailedMessageInfo failedMessage)
    {
        return JsonSerializer.Serialize(failedMessage, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    /// <summary>
    /// Deserializa FailedMessageInfo do JSON
    /// </summary>
    public static FailedMessageInfo? FromJson(string json)
    {
        return JsonSerializer.Deserialize<FailedMessageInfo>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Adiciona uma nova tentativa de falha ao histórico
    /// </summary>
    public static void AddFailureAttempt(this FailedMessageInfo failedMessage, Exception exception, string handlerType)
    {
        failedMessage.AttemptCount++;
        failedMessage.LastAttemptAt = DateTime.UtcNow;
        failedMessage.LastFailureReason = exception.Message;
        failedMessage.LastStackTrace = exception.StackTrace ?? string.Empty;

        failedMessage.FailureHistory.Add(new FailureAttempt
        {
            AttemptNumber = failedMessage.AttemptCount,
            AttemptedAt = DateTime.UtcNow,
            ExceptionType = exception.GetType().FullName ?? "Unknown",
            ExceptionMessage = exception.Message,
            StackTrace = exception.StackTrace ?? string.Empty,
            HandlerType = handlerType
        });
    }

    /// <summary>
    /// Classifica o tipo de falha baseado na exceção
    /// </summary>
    public static EFailureType ClassifyFailure(this Exception exception)
    {
        var exceptionType = exception.GetType().FullName ?? string.Empty;

        // Falhas permanentes - não deve tentar novamente
        string[] permanentExceptions = {
            "System.ArgumentException",
            "System.ArgumentNullException",
            "System.FormatException",
            "System.InvalidOperationException",
            "MeAjudaAi.Shared.Exceptions.BusinessRuleException",
            "MeAjudaAi.Shared.Exceptions.DomainException",
            "MeAjudaAi.Shared.Exceptions.ValidationException"
        };

        if (permanentExceptions.Contains(exceptionType))
            return EFailureType.Permanent;

        // Falhas temporárias - retry recomendado
        string[] transientExceptions = {
            "System.TimeoutException",
            "System.Net.Http.HttpRequestException",
            "Npgsql.PostgresException",
            "Microsoft.Data.SqlClient.SqlException",
            "System.Net.Sockets.SocketException",
            "System.IO.IOException"
        };

        if (transientExceptions.Contains(exceptionType))
            return EFailureType.Transient;

        // Falhas críticas
        if (exception is OutOfMemoryException or StackOverflowException)
            return EFailureType.Critical;

        return EFailureType.Unknown;
    }
}
