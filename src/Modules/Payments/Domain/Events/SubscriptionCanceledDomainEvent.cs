using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Payments.Domain.Events;

[ExcludeFromCodeCoverage]
public record SubscriptionCanceledDomainEvent(
    Guid SubscriptionId, 
    Guid ProviderId
) : DomainEvent(SubscriptionId, DomainConstants.InitialVersion);
