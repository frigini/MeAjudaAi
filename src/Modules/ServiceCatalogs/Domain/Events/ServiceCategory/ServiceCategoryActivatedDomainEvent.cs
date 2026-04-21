using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.ServiceCategory;

[ExcludeFromCodeCoverage]

public sealed record ServiceCategoryActivatedDomainEvent(ServiceCategoryId CategoryId)
    : DomainEvent(CategoryId.Value, Version: 1);
