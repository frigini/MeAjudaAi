using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class GetBookingsByProviderQueryHandler(
    IBookingRepository bookingRepository,
    IProviderScheduleRepository scheduleRepository,
    ILogger<GetBookingsByProviderQueryHandler> logger) : IQueryHandler<GetBookingsByProviderQuery, Result<IReadOnlyList<BookingDto>>>
{
    public async Task<Result<IReadOnlyList<BookingDto>>> HandleAsync(GetBookingsByProviderQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting bookings for provider {ProviderId}", query.ProviderId);

        // Prepara parâmetros de paginação e filtros
        var pageNumber = query.Page ?? 1;
        var pageSize = query.PageSize ?? 10;
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
        var schedule = await scheduleRepository.GetByProviderIdAsync(query.ProviderId, cancellationToken);
        var tz = ResolveTimeZone(schedule?.TimeZoneId);

        // Mapeia para DTOs garantindo o fuso horário correto
        var dtos = bookings.Select(booking =>
        {
            var startDate = booking.Date.ToDateTime(booking.TimeSlot.Start);
            var endDate = booking.Date.ToDateTime(booking.TimeSlot.End);

            return new BookingDto(
                booking.Id,
                booking.ProviderId,
                booking.ClientId,
                booking.ServiceId,
                TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(startDate, tz), tz),
                TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(endDate, tz), tz),
                booking.Status,
                booking.RejectionReason,
                booking.CancellationReason);
        }).ToList();

        return Result<IReadOnlyList<BookingDto>>.Success(dtos.AsReadOnly());
    }

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch
            {
                // Ignora e tenta fallback
            }
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
        catch
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }
    }
}
