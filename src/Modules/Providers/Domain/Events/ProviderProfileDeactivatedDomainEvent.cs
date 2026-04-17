using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um perfil de prestador de serviços é desativado.
/// </summary>
[ExcludeFromCodeCoverage]
public record ProviderProfileDeactivatedDomainEvent(
    Guid AggregateId,
    int Version,
    string? UpdatedBy) : DomainEvent(AggregateId, Version);
