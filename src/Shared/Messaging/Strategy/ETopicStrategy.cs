namespace MeAjudaAi.Shared.Messaging.Strategy;

public enum ETopicStrategy
{
    None = 0,
    SingleWithFilters,    // Um tópico + filtros (recomendado para monolito modular)
    MultipleByDomain,     // Múltiplos tópicos por domínio
    Hybrid                // Híbrido: tópicos críticos separados + default
}
