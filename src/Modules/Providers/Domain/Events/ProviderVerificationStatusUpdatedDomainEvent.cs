using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Providers.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando o status de verificação do prestador de serviços é atualizado.
/// </summary>
/// <param name="AggregateId">Identificador único do prestador de serviços</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="PreviousStatus">Status anterior</param>
/// <param name="NewStatus">Novo status</param>
/// <param name="UpdatedBy">Quem fez a atualização</param>
public record ProviderVerificationStatusUpdatedDomainEvent(
    Guid AggregateId,
    int Version,
    EVerificationStatus PreviousStatus,
    EVerificationStatus NewStatus,
    string? UpdatedBy
) : DomainEvent(AggregateId, Version);