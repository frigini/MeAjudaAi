using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Providers;

/// <summary>
/// Evento de integração disparado quando os serviços de um prestador são atualizados.
/// </summary>
/// <param name="Source">Módulo que originou o evento</param>
/// <param name="ProviderId">ID do prestador</param>
/// <param name="ServiceIds">Lista de IDs de serviços do prestador (pode ser vazio se precisar buscar tudo)</param>
public sealed record ProviderServicesUpdatedIntegrationEvent(
    string Source,
    Guid ProviderId,
    IReadOnlyList<Guid> ServiceIds) : IntegrationEvent(Source);
