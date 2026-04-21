using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;

[ExcludeFromCodeCoverage]

public sealed record ServiceCategoryChangedDomainEvent(
    ServiceId ServiceId,
    ServiceCategoryId OldCategoryId,
    ServiceCategoryId NewCategoryId)
    : DomainEvent(ServiceId.Value, Version: 1);
