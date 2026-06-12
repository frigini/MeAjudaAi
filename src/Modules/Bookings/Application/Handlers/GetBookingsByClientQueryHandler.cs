using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Services;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;

namespace MeAjudaAi.Modules.Bookings.Application.Handlers;

public sealed class GetBookingsByClientQueryHandler(
    IBookingQueries bookingQueries,
    IProviderScheduleQueries scheduleQueries,
    ILogger<GetBookingsByClientQueryHandler> logger) : IQueryHandler<GetBookingsByClientQuery, Result<PagedResult<ModuleBookingDto>>>
{
    public async Task<Result<PagedResult<ModuleBookingDto>>> HandleAsync(GetBookingsByClientQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting bookings for client {ClientId}", query.ClientId);

        var pageNumber = query.Page ?? 1;
        var pageSize = query.PageSize ?? 10;
        var fromDate = query.From.HasValue ? DateOnly.FromDateTime(query.From.Value) : (DateOnly?)null;
        var toDate = query.To.HasValue ? DateOnly.FromDateTime(query.To.Value) : (DateOnly?)null;

        var (bookings, totalCount) = await bookingQueries.GetByClientIdPagedAsync(
            query.ClientId,
            fromDate,
            toDate,
            pageNumber,
            pageSize,
            cancellationToken);

        var dtos = new List<ModuleBookingDto>();
        var scheduleCache = new Dictionary<Guid, Domain.Entities.ProviderSchedule?>();

        foreach (var booking in bookings)
        {
            if (!scheduleCache.TryGetValue(booking.ProviderId, out var schedule))
            {
                schedule = await scheduleQueries.GetByProviderIdReadOnlyAsync(booking.ProviderId, cancellationToken);
                scheduleCache[booking.ProviderId] = schedule;
            }

            var tz = TimeZoneResolver.ResolveTimeZone(schedule?.TimeZoneId, logger);
            var dtoResult = TimeZoneResolver.CreateValidatedBookingDto(booking, tz!, logger);

            if (dtoResult.IsFailure)
            {
                return Result<PagedResult<ModuleBookingDto>>.Failure(dtoResult.Error);
            }

            dtos.Add(dtoResult.Value!);
        }

        return Result<PagedResult<ModuleBookingDto>>.Success(new PagedResult<ModuleBookingDto>
        {
            Items = dtos.AsReadOnly(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalCount
        });
    }
}
