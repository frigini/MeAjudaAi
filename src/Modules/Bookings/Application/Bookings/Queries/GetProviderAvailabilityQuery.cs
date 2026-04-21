using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;

public record GetProviderAvailabilityQuery(
    Guid ProviderId,
    DateOnly Date,
    Guid CorrelationId = default) : IQuery<Result<AvailabilityDto>>;
