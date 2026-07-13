using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.DeadLetter.Models;

/// <summary>
/// Informações sobre uma mensagem que falhou durante o processamento
/// </summary>
[ExcludeFromCodeCoverage]
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
