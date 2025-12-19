namespace MeAjudaAi.Modules.Documents.Application.Constants;

/// <summary>
/// Identificadores de modelos do Azure AI Document Intelligence
/// </summary>
public static class ModelIds
{
    /// <summary>
    /// Modelo pré-construído para documentos de identidade (RG, CNH, etc)
    /// </summary>
    public const string IdentityDocument = "prebuilt-idDocument";

    /// <summary>
    /// Modelo pré-construído genérico para documentos não estruturados
    /// </summary>
    public const string GenericDocument = "prebuilt-document";

    /// <summary>
    /// Modelo pré-construído para faturas e recibos
    /// </summary>
    public const string Invoice = "prebuilt-invoice";

    /// <summary>
    /// Modelo pré-construído para recibos
    /// </summary>
    public const string Receipt = "prebuilt-receipt";
}
