using System.Text.Json.Serialization;

namespace MeAjudaAi.Contracts.Modules.SearchProviders.Enums;

/// <summary>
/// Enumeração de níveis de assinatura para API do módulo.
/// Os valores devem corresponder a MeAjudaAi.Modules.SearchProviders.Domain.Enums.ESubscriptionTier.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ESubscriptionTier
{
    Free = 0,
    Standard = 1,
    Gold = 2,
    Platinum = 3
}

