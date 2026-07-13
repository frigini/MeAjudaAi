using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.DeadLetter.Models;

/// <summary>
/// Metadados do ambiente onde a falha ocorreu
/// </summary>
[ExcludeFromCodeCoverage]
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
