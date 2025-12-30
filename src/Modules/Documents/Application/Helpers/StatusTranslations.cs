using MeAjudaAi.Modules.Documents.Domain.Enums;

namespace MeAjudaAi.Modules.Documents.Application.Helpers;

/// <summary>
/// Helper para tradução de status de documentos para português.
/// </summary>
public static class StatusTranslations
{
    /// <summary>
    /// Converte o status do documento para sua descrição em português.
    /// </summary>
    /// <param name="status">Status do documento</param>
    /// <returns>Descrição em português do status</returns>
    public static string ToPortuguese(this EDocumentStatus status) => status switch
    {
        EDocumentStatus.PendingVerification => "Verificação Pendente",
        EDocumentStatus.Uploaded => "Enviado",
        EDocumentStatus.Rejected => "Rejeitado",
        EDocumentStatus.Verified => "Verificado",
        _ => status.ToString()
    };
}
