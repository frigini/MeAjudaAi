using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Services;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Handlers;

public sealed class GetBookingsByProviderQueryHandler(
    IBookingQueries bookingQueries,
    IProviderScheduleQueries scheduleQueries,
    ILogger<GetBookingsByProviderQueryHandler> logger) : IQueryHandler<GetBookingsByProviderQuery, Result<PagedResult<BookingDto>>>
{
    public async Task<Result<PagedResult<BookingDto>>> HandleAsync(GetBookingsByProviderQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting bookings for provider {ProviderId}", query.ProviderId);

        var pageNumber = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var fromDate = query.From.HasValue ? DateOnly.FromDateTime(query.From.Value) : (DateOnly?)null;
        var toDate = query.To.HasValue ? DateOnly.FromDateTime(query.To.Value) : (DateOnly?)null;

        var (bookings, totalCount) = await bookingQueries.GetByProviderIdPagedAsync(
            query.ProviderId, 
            fromDate, 
            toDate, 
            pageNumber, 
            pageSize, 
            cancellationToken);

        var schedule = await scheduleQueries.GetByProviderIdReadOnlyAsync(query.ProviderId, cancellationToken);
        var tz = TimeZoneResolver.ResolveTimeZone(schedule?.TimeZoneId, logger);

        if (tz == null)
        {
            logger.LogError("Could not resolve time zone for provider {ProviderId}", query.ProviderId);
            return Result<PagedResult<BookingDto>>.Failure(Error.Internal("Não foi possível processar o fuso horário do prestador."));
        }

        var dtos = new List<BookingDto>();
        foreach (var booking in bookings)
        {
            var dtoResult = TimeZoneResolver.CreateValidatedBookingDto(booking, tz, logger);
            if (dtoResult.IsFailure)
            {
                return Result<PagedResult<BookingDto>>.Failure(dtoResult.Error);
            }
            dtos.Add(dtoResult.Value!);
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
