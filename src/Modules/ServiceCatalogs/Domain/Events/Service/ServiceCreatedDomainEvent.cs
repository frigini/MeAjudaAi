using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;

[ExcludeFromCodeCoverage]

public sealed record ServiceCreatedDomainEvent(ServiceId ServiceId, ServiceCategoryId CategoryId)
    : DomainEvent(ServiceId.Value, Version: 1);
