using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.DeadLetter;

/// <summary>
/// Informações sobre uma tentativa de processamento que falhou
/// </summary>
[ExcludeFromCodeCoverage]
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
