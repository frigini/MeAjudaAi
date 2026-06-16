using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Bookings.Application.Queries;

public record GetProviderAvailabilityQuery(
    Guid ProviderId,
    DateOnly Date,
    Guid CorrelationId) : IQuery<Result<AvailabilityDto>>;
