using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;

public record GetBookingsByProviderQuery(
    Guid ProviderId,
    Guid CorrelationId,
    int? Page = 1,
    int? PageSize = 10,
    DateTime? From = null,
    DateTime? To = null) : IQuery<Result<PagedResult<BookingDto>>>;
