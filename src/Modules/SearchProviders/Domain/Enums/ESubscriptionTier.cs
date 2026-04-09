namespace MeAjudaAi.Modules.SearchProviders.Domain.Enums;

/// <summary>
/// Representa o tier de assinatura de um provedor.
/// Tiers mais altos recebem melhor posicionamento nos resultados de busca.
/// </summary>
public enum ESubscriptionTier
{
    /// <summary>
    /// Tier gratuito - listagem básica
    /// </summary>
    Free = 0,

    /// <summary>
    /// Tier padrão - listagem aprimorada com recursos adicionais
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Tier ouro - listagem premium com prioridade nos resultados de busca
    /// </summary>
    Gold = 2,

    /// <summary>
    /// Tier platina - maior prioridade nos resultados de busca com máxima visibilidade
    /// </summary>
    Platinum = 3
}
