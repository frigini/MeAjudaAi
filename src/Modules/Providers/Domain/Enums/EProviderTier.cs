namespace MeAjudaAi.Modules.Providers.Domain.Enums;

/// <summary>
/// Tier de assinatura do prestador de serviços.
/// </summary>
/// <remarks>
/// Controla o nível de visibilidade e benefícios do prestador na plataforma.
/// A promoção de tier é automática via webhook do Stripe (módulo de pagamentos futuro).
/// - Standard: plano gratuito (padrão no cadastro)
/// - Silver/Gold/Platinum: planos pagos
/// </remarks>
public enum EProviderTier
{
    /// <summary>
    /// Plano gratuito — padrão para todos os novos prestadores.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Plano pago nível 1 — maior visibilidade nos resultados de busca.
    /// </summary>
    Silver = 1,

    /// <summary>
    /// Plano pago nível 2 — destaque premium nos resultados de busca.
    /// </summary>
    Gold = 2,

    /// <summary>
    /// Plano pago nível 3 — máxima visibilidade e benefícios exclusivos.
    /// </summary>
    Platinum = 3
}
