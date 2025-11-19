using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Catalogs.Domain.Events.Service;

public sealed record ServiceCategoryChangedDomainEvent(
    ServiceId ServiceId,
    ServiceCategoryId OldCategoryId,
    ServiceCategoryId NewCategoryId)
    : DomainEvent(ServiceId.Value, Version: 1);
