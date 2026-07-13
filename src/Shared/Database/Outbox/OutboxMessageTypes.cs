using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Database.Outbox;

/// <summary>
/// Define os tipos padrão de mensagens do Outbox.
/// </summary>
[ExcludeFromCodeCoverage]
public static class OutboxMessageTypes
{
    /// <summary>
    /// Tipo de mensagem para processamento de verificação de documentos.
    /// </summary>
    public const string DocumentVerification = "DocumentVerification";
}
