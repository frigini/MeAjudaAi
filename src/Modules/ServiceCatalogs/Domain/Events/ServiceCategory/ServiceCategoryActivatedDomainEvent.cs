using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.ServiceCategory;

public sealed record ServiceCategoryActivatedDomainEvent(ServiceCategoryId CategoryId)
    : DomainEvent(CategoryId.Value, Version: 1);
