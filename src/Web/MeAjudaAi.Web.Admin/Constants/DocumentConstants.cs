namespace MeAjudaAi.Web.Admin.Constants;

/// <summary>
/// Constantes para status de documentos no processo de verificação.
/// Replica o enum EDocumentStatus do módulo Documents.
/// </summary>
public static class DocumentStatus
{
    /// <summary>
    /// Documento foi enviado mas ainda não processado
    /// </summary>
    public const int Uploaded = 1;

    /// <summary>
    /// Documento está aguardando verificação (OCR ou verificação manual)
    /// </summary>
    public const int PendingVerification = 2;

    /// <summary>
    /// Documento foi verificado e aprovado
    /// </summary>
    public const int Verified = 3;

    /// <summary>
    /// Documento foi rejeitado (dados inválidos, ilegível, etc)
    /// </summary>
    public const int Rejected = 4;

    /// <summary>
    /// Falha no processamento do documento
    /// </summary>
    public const int Failed = 5;

    /// <summary>
    /// Retorna o nome de exibição localizado para o status do documento.
    /// </summary>
    /// <param name="status">Valor numérico do status</param>
    /// <returns>Nome em português do status</returns>
    public static string ToDisplayName(int status) => status switch
    {
        Uploaded => "Enviado",
        PendingVerification => "Aguardando Verificação",
        Verified => "Verificado",
        Rejected => "Rejeitado",
        Failed => "Falha no Processamento",
        _ => "Desconhecido"
    };

    /// <summary>
    /// Retorna o nome de exibição localizado para o status do documento (versão string).
    /// </summary>
    /// <param name="status">Status como string</param>
    /// <returns>Nome em português do status</returns>
    public static string ToDisplayName(string status) => status switch
    {
        "Uploaded" => "Enviado",
        "PendingVerification" => "Aguardando Verificação",
        "Verified" => "Verificado",
        "Rejected" => "Rejeitado",
        "Failed" => "Falha no Processamento",
        _ => status
    };

    /// <summary>
    /// Retorna a cor MudBlazor apropriada para o status do documento.
    /// </summary>
    /// <param name="status">Valor numérico do status</param>
    /// <returns>Cor do MudBlazor</returns>
    public static MudBlazor.Color ToColor(int status) => status switch
    {
        Verified => MudBlazor.Color.Success,
        PendingVerification => MudBlazor.Color.Warning,
        Uploaded => MudBlazor.Color.Info,
        Rejected => MudBlazor.Color.Error,
        Failed => MudBlazor.Color.Error,
        _ => MudBlazor.Color.Default
    };

    /// <summary>
    /// Retorna a cor MudBlazor apropriada para o status do documento (versão string).
    /// </summary>
    /// <param name="status">Status como string</param>
    /// <returns>Cor do MudBlazor</returns>
    public static MudBlazor.Color ToColor(string status) => status switch
    {
        "Verified" => MudBlazor.Color.Success,
        "PendingVerification" => MudBlazor.Color.Warning,
        "Uploaded" => MudBlazor.Color.Info,
        "Rejected" => MudBlazor.Color.Error,
        "Failed" => MudBlazor.Color.Error,
        _ => MudBlazor.Color.Default
    };
}

/// <summary>
/// Constantes para tipos de documentos suportados pelo sistema.
/// Replica o enum EDocumentType do módulo Documents.
/// </summary>
public static class DocumentType
{
    /// <summary>
    /// Documentos de identidade (RG, CPF, CNH)
    /// </summary>
    public const int IdentityDocument = 1;

    /// <summary>
    /// Comprovante de residência
    /// </summary>
    public const int ProofOfResidence = 2;

    /// <summary>
    /// Certidão de antecedentes criminais
    /// </summary>
    public const int CriminalRecord = 3;

    /// <summary>
    /// Outros documentos
    /// </summary>
    public const int Other = 99;

    /// <summary>
    /// Retorna o nome de exibição localizado para o tipo de documento.
    /// </summary>
    /// <param name="type">Valor numérico do tipo</param>
    /// <returns>Nome em português do tipo</returns>
    public static string ToDisplayName(int type) => type switch
    {
        IdentityDocument => "Documento de Identidade",
        ProofOfResidence => "Comprovante de Residência",
        CriminalRecord => "Certidão de Antecedentes",
        Other => "Outros",
        _ => "Desconhecido"
    };

    /// <summary>
    /// Retorna todos os tipos válidos de documento.
    /// </summary>
    /// <returns>Lista de tuplas (value, displayName)</returns>
    public static IEnumerable<(int Value, string DisplayName)> GetAll() =>
        new[]
        {
            (IdentityDocument, ToDisplayName(IdentityDocument)),
            (ProofOfResidence, ToDisplayName(ProofOfResidence)),
            (CriminalRecord, ToDisplayName(CriminalRecord)),
            (Other, ToDisplayName(Other))
        };
}
