using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;

public sealed record ServiceCategoryChangedDomainEvent(
    ServiceId ServiceId,
    ServiceCategoryId OldCategoryId,
    ServiceCategoryId NewCategoryId)
    : DomainEvent(ServiceId.Value, Version: 1);
