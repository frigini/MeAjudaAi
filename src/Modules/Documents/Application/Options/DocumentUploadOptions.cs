namespace MeAjudaAi.Modules.Documents.Application.Options;

/// <summary>
/// Opções de configuração para upload de documentos.
/// </summary>
public class DocumentUploadOptions
{
    /// <summary>
    /// Tamanho máximo global de arquivo em bytes.
    /// Usado quando não há limite específico para o tipo de documento.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB default

    /// <summary>
    /// Limites de tamanho específicos por tipo de documento (em bytes).
    /// Sobrescreve MaxFileSizeBytes para tipos específicos.
    /// Usar nomes exatos do enum EDocumentType.
    /// </summary>
    public Dictionary<string, long> MaxFileSizeByDocumentType { get; set; } = new()
    {
        // Documentos de identidade podem ser maiores (fotos de alta qualidade)
        ["IdentityDocument"] = 15 * 1024 * 1024, // 15MB
        
        // Comprovantes geralmente são PDFs menores
        ["ProofOfResidence"] = 5 * 1024 * 1024,   // 5MB
        
        // Certidão de antecedentes criminais
        ["CriminalRecord"] = 8 * 1024 * 1024,     // 8MB
        
        // Outros documentos usam limite global (10MB)
    };

    /// <summary>
    /// Content-types permitidos para upload.
    /// </summary>
    public string[] AllowedContentTypes { get; set; } = 
    [
        "image/jpeg",
        "image/png",
        "image/jpg",
        "application/pdf"
    ];

    /// <summary>
    /// Obtém o tamanho máximo permitido para um tipo de documento específico.
    /// </summary>
    public long GetMaxFileSizeForDocumentType(string documentType)
    {
        return MaxFileSizeByDocumentType.TryGetValue(documentType, out var maxSize)
            ? maxSize
            : MaxFileSizeBytes;
    }
}
