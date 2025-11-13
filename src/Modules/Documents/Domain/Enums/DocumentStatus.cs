namespace MeAjudaAi.Modules.Documents.Domain.Enums;

/// <summary>
/// Status de um documento no processo de verificação
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    /// Documento foi enviado mas ainda não processado
    /// </summary>
    Uploaded = 1,
    
    /// <summary>
    /// Documento está aguardando verificação (OCR ou verificação manual)
    /// </summary>
    PendingVerification = 2,
    
    /// <summary>
    /// Documento foi verificado e aprovado
    /// </summary>
    Verified = 3,
    
    /// <summary>
    /// Documento foi rejeitado (dados inválidos, ilegível, etc)
    /// </summary>
    Rejected = 4,
    
    /// <summary>
    /// Falha no processamento do documento
    /// </summary>
    Failed = 5
}
