using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Modules.Providers.Domain.Enums;

/// <summary>
/// Métodos de extensão para conversão entre <see cref="EProviderTier"/> e os papéis de Keycloak <see cref="UserRoles"/>.
/// Centraliza a conversão para evitar deriva de strings e quebras de integridade caso novos planos sejam adicionados.
/// </summary>
public static class ProviderTierExtensions
{
    /// <summary>
    /// Retorna a representação de string canônica definida em <see cref="UserRoles"/> para um dado nível.
    /// </summary>
    public static string ToRoleString(this EProviderTier tier) => tier switch
    {
        EProviderTier.Standard => UserRoles.ProviderStandard,
        EProviderTier.Silver => UserRoles.ProviderSilver,
        EProviderTier.Gold => UserRoles.ProviderGold,
        EProviderTier.Platinum => UserRoles.ProviderPlatinum,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null)
    };

    /// <summary>
    /// Tenta interpretar uma string de Papel no Keycloak na entidade de Nível correta do Domínio <see cref="EProviderTier"/>.
    /// </summary>
    public static bool TryParseRole(string role, out EProviderTier tier)
    {
        switch (role?.ToLowerInvariant())
        {
            case UserRoles.ProviderStandard:
                tier = EProviderTier.Standard;
                return true;
            case UserRoles.ProviderSilver:
                tier = EProviderTier.Silver;
                return true;
            case UserRoles.ProviderGold:
                tier = EProviderTier.Gold;
                return true;
            case UserRoles.ProviderPlatinum:
                tier = EProviderTier.Platinum;
                return true;
            default:
                tier = default;
                return false;
        }
    }
}
