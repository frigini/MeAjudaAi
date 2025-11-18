using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Catalogs.Domain.Events;

public sealed record ServiceCreatedDomainEvent(ServiceId ServiceId, ServiceCategoryId CategoryId)
    : DomainEvent(ServiceId.Value, Version: 1);

public sealed record ServiceUpdatedDomainEvent(ServiceId ServiceId)
    : DomainEvent(ServiceId.Value, Version: 1);

public sealed record ServiceActivatedDomainEvent(ServiceId ServiceId)
    : DomainEvent(ServiceId.Value, Version: 1);

public sealed record ServiceDeactivatedDomainEvent(ServiceId ServiceId)
    : DomainEvent(ServiceId.Value, Version: 1);

public sealed record ServiceCategoryChangedDomainEvent(
    ServiceId ServiceId,
    ServiceCategoryId OldCategoryId,
    ServiceCategoryId NewCategoryId)
    : DomainEvent(ServiceId.Value, Version: 1);
