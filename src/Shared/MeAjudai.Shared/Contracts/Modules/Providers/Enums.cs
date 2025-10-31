namespace MeAjudaAi.Shared.Contracts.Modules.Providers;

/// <summary>
/// Tipo de prestador de serviços (Individual ou Company)
/// </summary>
public enum EProviderType
{
    Individual = 1,
    Company = 2
}

/// <summary>
/// Status de verificação do prestador de serviços
/// </summary>
public enum EVerificationStatus
{
    Pending = 1,
    InProgress = 2,
    Verified = 3,
    Rejected = 4,
    Suspended = 5
}

/// <summary>
/// Tipos de documentos suportados pelo sistema
/// </summary>
public enum EDocumentType
{
    CPF = 1,
    CNPJ = 2,
    RG = 3,
    CNH = 4,
    Passport = 5,
    Other = 99
}