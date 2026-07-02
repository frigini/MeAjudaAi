using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Application.Services;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar consultas de bookings de um prestador de serviços.
/// </summary>
/// <param name="bookingQueries">Queries de acesso a dados de bookings.</param>
/// <param name="scheduleQueries">Queries de acesso a dados de agenda do prestador.</param>
/// <param name="logger">Logger estruturado.</param>
/// <param name="localizer">Localizador de strings para mensagens de erro.</param>
/// <returns>
/// Um <see cref="Result{PagedResult}"/> contendo a lista paginada de <see cref="ModuleBookingDto"/>
/// em caso de sucesso, ou um <see cref="Error"/> descritivo em caso de falha.
/// </returns>
public sealed class GetBookingsByProviderQueryHandler(
    IBookingQueries bookingQueries,
    IProviderScheduleQueries scheduleQueries,
    ILogger<GetBookingsByProviderQueryHandler> logger,
    IStringLocalizer<Strings> localizer) : IQueryHandler<GetBookingsByProviderQuery, Result<PagedResult<ModuleBookingDto>>>
{
    public async Task<Result<PagedResult<ModuleBookingDto>>> HandleAsync(GetBookingsByProviderQuery query, CancellationToken cancellationToken = default)
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
            return Result<PagedResult<ModuleBookingDto>>.Failure(Error.Internal(localizer["ProviderScheduleLoadError"]));
        }

        var dtos = new List<ModuleBookingDto>();
        foreach (var booking in bookings)
        {
            var dtoResult = TimeZoneResolver.CreateValidatedBookingDto(booking, tz, logger);
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
