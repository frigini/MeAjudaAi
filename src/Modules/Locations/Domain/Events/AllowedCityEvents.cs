using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Locations.Domain.Events;

public record AllowedCityCreatedDomainEvent(Guid CityId, string CityName, string StateSigla) : DomainEvent(CityId);
public record AllowedCityUpdatedDomainEvent(Guid CityId, string CityName, string StateSigla) : DomainEvent(CityId);
public record AllowedCityDeletedDomainEvent(Guid CityId, string CityName, string StateSigla) : DomainEvent(CityId);
