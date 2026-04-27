using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Shared.Queries;

using MeAjudaAi.Contracts.Models;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;

public record GetBookingsByClientQuery(
    Guid ClientId,
    Guid CorrelationId,
    int? Page = 1,
    int? PageSize = 10,
    DateTime? From = null,
    DateTime? To = null) : IQuery<Result<PagedResult<BookingDto>>>;
