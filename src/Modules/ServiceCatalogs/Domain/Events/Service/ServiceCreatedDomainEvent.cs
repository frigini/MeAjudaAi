using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;

public sealed record ServiceCreatedDomainEvent(ServiceId ServiceId, ServiceCategoryId CategoryId)
    : DomainEvent(ServiceId.Value, Version: 1);
