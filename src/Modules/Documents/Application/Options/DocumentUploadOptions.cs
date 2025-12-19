namespace MeAjudaAi.Modules.Documents.Application.Options;

/// <summary>
/// Opções de configuração para upload de documentos.
/// </summary>
public class DocumentUploadOptions
{
    /// <summary>
    /// Tamanho máximo de arquivo em bytes.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB default

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
}
