using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Payments.Domain.Events;

[ExcludeFromCodeCoverage]
public record SubscriptionActivatedDomainEvent(
    Guid SubscriptionId, 
    Guid ProviderId, 
    string ExternalSubscriptionId
) : DomainEvent(SubscriptionId, DomainConstants.InitialVersion);
