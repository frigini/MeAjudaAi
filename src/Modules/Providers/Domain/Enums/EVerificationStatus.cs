namespace MeAjudaAi.Modules.Providers.Domain.Enums;

/// <summary>
/// Status de verificação do prestador de serviços.
/// </summary>
public enum EVerificationStatus
{
    Pending = 1,
    InProgress = 2,
    Verified = 3,
    Rejected = 4,
    Suspended = 5
}