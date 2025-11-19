using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Catalogs.Domain.Events.ServiceCategory;

public sealed record ServiceCategoryActivatedDomainEvent(ServiceCategoryId CategoryId)
    : DomainEvent(CategoryId.Value, Version: 1);
