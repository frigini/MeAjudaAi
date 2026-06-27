using MeAjudaAi.Contracts.Modules.SearchProviders.Enums;
using DomainEnums = MeAjudaAi.Modules.SearchProviders.Domain.Enums;

namespace MeAjudaAi.Modules.SearchProviders.Application.Mappers;

/// <summary>
/// Mapeia enums de SubscriptionTier entre o domínio e o contrato do módulo.
/// </summary>
public static class SubscriptionTierMapper
{
    /// <summary>
    /// Mapeia o enum de tier do módulo para o enum de tier do domínio.
    /// </summary>
    public static DomainEnums.ESubscriptionTier ToDomainTier(this ESubscriptionTier tier) => tier switch
    {
        ESubscriptionTier.Free => DomainEnums.ESubscriptionTier.Free,
        ESubscriptionTier.Standard => DomainEnums.ESubscriptionTier.Standard,
        ESubscriptionTier.Gold => DomainEnums.ESubscriptionTier.Gold,
        ESubscriptionTier.Platinum => DomainEnums.ESubscriptionTier.Platinum,
        _ => throw new NotSupportedException($"Subscription tier '{tier}' is not supported.")
    };

    /// <summary>
    /// Mapeia o enum de tier do domínio para o enum de tier do módulo.
    /// </summary>
    public static ESubscriptionTier ToModuleTier(this DomainEnums.ESubscriptionTier tier) => tier switch
    {
        DomainEnums.ESubscriptionTier.Free => ESubscriptionTier.Free,
        DomainEnums.ESubscriptionTier.Standard => ESubscriptionTier.Standard,
        DomainEnums.ESubscriptionTier.Gold => ESubscriptionTier.Gold,
        DomainEnums.ESubscriptionTier.Platinum => ESubscriptionTier.Platinum,
        _ => throw new NotSupportedException($"Domain subscription tier '{tier}' is not supported.")
    };
}
