namespace MeAjudaAi.Shared.Messaging.Strategy;

public enum ETopicStrategy
{
    SingleWithFilters,    // Um tópico + filtros (recomendado para monolito modular)
    MultipleByDomain,     // Múltiplos tópicos por domínio
    Hybrid                // Híbrido: tópicos críticos separados + default
}
