using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Bookings.Application.Queries;

public record GetBookingsByClientQuery(
    Guid ClientId,
    Guid CorrelationId,
    int? Page = 1,
    int? PageSize = 10,
    DateTime? From = null,
    DateTime? To = null) : IQuery<Result<PagedResult<ModuleBookingDto>>>;
