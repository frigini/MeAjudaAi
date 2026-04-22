using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Application.Common;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class GetBookingsByClientQueryHandler(
    IBookingRepository bookingRepository,
    IProviderScheduleRepository scheduleRepository,
    ILogger<GetBookingsByClientQueryHandler> logger) : IQueryHandler<GetBookingsByClientQuery, Result<PagedResult<BookingDto>>>
{
    public async Task<Result<PagedResult<BookingDto>>> HandleAsync(GetBookingsByClientQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting bookings for client {ClientId}", query.ClientId);

        var pageNumber = query.Page ?? 1;
        var pageSize = query.PageSize ?? 10;
        var fromDate = query.From.HasValue ? DateOnly.FromDateTime(query.From.Value) : (DateOnly?)null;
        var toDate = query.To.HasValue ? DateOnly.FromDateTime(query.To.Value) : (DateOnly?)null;

        var (bookings, totalCount) = await bookingRepository.GetByClientIdPagedAsync(
            query.ClientId,
            fromDate,
            toDate,
            pageNumber,
            pageSize,
            cancellationToken);

        var dtos = new List<BookingDto>();
        var scheduleCache = new Dictionary<Guid, Domain.Entities.ProviderSchedule?>();

        foreach (var booking in bookings)
        {
            if (!scheduleCache.TryGetValue(booking.ProviderId, out var schedule))
            {
                schedule = await scheduleRepository.GetByProviderIdReadOnlyAsync(booking.ProviderId, cancellationToken);
                scheduleCache[booking.ProviderId] = schedule;
            }

            var tz = TimeZoneResolver.ResolveTimeZone(schedule?.TimeZoneId, logger);
            var dtoResult = TimeZoneResolver.CreateValidatedBookingDto(booking, tz!, logger);

            if (dtoResult.IsFailure)
            {
                return Result<PagedResult<BookingDto>>.Failure(dtoResult.Error);
            }

            dtos.Add(dtoResult.Value);
        }

        return Result<PagedResult<BookingDto>>.Success(new PagedResult<BookingDto>
        {
            Items = dtos.AsReadOnly(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalCount
        });
    }
}
