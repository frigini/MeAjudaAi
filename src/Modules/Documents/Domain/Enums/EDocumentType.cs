namespace MeAjudaAi.Modules.Documents.Domain.Enums;

/// <summary>
/// Tipos de documentos suportados pelo sistema
/// </summary>
public enum EDocumentType
{
    /// <summary>
    /// Documentos de identidade (RG, CPF, CNH)
    /// </summary>
    IdentityDocument = 1,
    
    /// <summary>
    /// Comprovante de residência
    /// </summary>
    ProofOfResidence = 2,
    
    /// <summary>
    /// Certidão de antecedentes criminais
    /// </summary>
    CriminalRecord = 3,
    
    /// <summary>
    /// Outros documentos
    /// </summary>
    Other = 99
}
