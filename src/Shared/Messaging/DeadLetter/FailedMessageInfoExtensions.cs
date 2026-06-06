namespace MeAjudaAi.Shared.Messaging.DeadLetter;

/// <summary>
/// Extensões para facilitar o trabalho com FailedMessageInfo
/// </summary>
public static class FailedMessageInfoExtensions
{
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

        // Falhas críticas - usar typeof para incluir subtipos
        if (exception is OutOfMemoryException or StackOverflowException)
            return EFailureType.Critical;

        return EFailureType.Unknown;
    }
}
