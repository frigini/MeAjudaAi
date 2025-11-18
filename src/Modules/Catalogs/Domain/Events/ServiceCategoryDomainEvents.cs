using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Catalogs.Domain.Events;

public sealed record ServiceCategoryCreatedDomainEvent(ServiceCategoryId CategoryId) 
    : DomainEvent(CategoryId.Value, Version: 1);

public sealed record ServiceCategoryUpdatedDomainEvent(ServiceCategoryId CategoryId) 
    : DomainEvent(CategoryId.Value, Version: 1);

public sealed record ServiceCategoryActivatedDomainEvent(ServiceCategoryId CategoryId) 
    : DomainEvent(CategoryId.Value, Version: 1);

public sealed record ServiceCategoryDeactivatedDomainEvent(ServiceCategoryId CategoryId) 
    : DomainEvent(CategoryId.Value, Version: 1);
