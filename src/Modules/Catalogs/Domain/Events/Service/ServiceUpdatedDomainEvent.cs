using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Catalogs.Domain.Events.Service;

public sealed record ServiceUpdatedDomainEvent(ServiceId ServiceId)
    : DomainEvent(ServiceId.Value, Version: 1);
