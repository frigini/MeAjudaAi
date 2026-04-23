using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Application.Common;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class GetBookingsByProviderQueryHandler(
    IBookingRepository bookingRepository,
    IProviderScheduleRepository scheduleRepository,
    ILogger<GetBookingsByProviderQueryHandler> logger) : IQueryHandler<GetBookingsByProviderQuery, Result<PagedResult<BookingDto>>>
{
    public async Task<Result<PagedResult<BookingDto>>> HandleAsync(GetBookingsByProviderQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting bookings for provider {ProviderId}", query.ProviderId);

        // Prepara parâmetros de paginação e filtros
        var pageNumber = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var fromDate = query.From.HasValue ? DateOnly.FromDateTime(query.From.Value) : (DateOnly?)null;
        var toDate = query.To.HasValue ? DateOnly.FromDateTime(query.To.Value) : (DateOnly?)null;

        // Busca os agendamentos paginados do repositório
        var (bookings, totalCount) = await bookingRepository.GetByProviderIdPagedAsync(
            query.ProviderId, 
            fromDate, 
            toDate, 
            pageNumber, 
            pageSize, 
            cancellationToken);

        // Resolve o fuso horário do prestador
        var schedule = await scheduleRepository.GetByProviderIdReadOnlyAsync(query.ProviderId, cancellationToken);
        var tz = TimeZoneResolver.ResolveTimeZone(schedule?.TimeZoneId, logger);

        if (tz == null)
        {
            logger.LogError("Could not resolve time zone for provider {ProviderId}", query.ProviderId);
            return Result<PagedResult<BookingDto>>.Failure(Error.Internal("Não foi possível processar o fuso horário do prestador."));
        }

        // Mapeia para DTOs garantindo o fuso horário correto
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
