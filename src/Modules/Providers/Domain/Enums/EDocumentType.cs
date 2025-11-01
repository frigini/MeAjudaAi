namespace MeAjudaAi.Modules.Providers.Domain.Enums;

/// <summary>
/// Tipos de documentos suportados pelo sistema.
/// </summary>
public enum EDocumentType
{
    /// <summary>
    /// Tipo não definido
    /// </summary>
    None = 0,

    CPF = 1,
    CNPJ = 2,
    RG = 3,
    CNH = 4,
    Passport = 5,
    Other = 99
}