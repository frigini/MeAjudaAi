namespace MeAjudaAi.Shared.Database.Outbox;

/// <summary>
/// Tipos de mensagens outbox utilizadas no sistema.
/// </summary>
public static class OutboxMessageTypes
{
    /// <summary>
    /// Tipo de mensagem outbox para verificação/OCR de documentos.
    /// </summary>
    public const string DocumentVerification = "DocumentVerification";
}