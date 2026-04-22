using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Common;

public static class TimeZoneResolver
{
    public static TimeZoneInfo ResolveTimeZone(string? timeZoneId, ILogger logger)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to resolve time zone {TimeZoneId}. Falling back.", timeZoneId);
            }
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resolve Windows Brazil time zone. Trying IANA.");
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch (Exception exIana)
            {
                logger.LogWarning(exIana, "Failed to resolve IANA Brazil time zone. Using local/UTC.");
                try
                {
                    return TimeZoneInfo.Local;
                }
                catch
                {
                    return TimeZoneInfo.Utc;
                }
            }
        }
    }

    public static Result<BookingDto> CreateValidatedBookingDto(Booking booking, TimeZoneInfo tz, ILogger logger)
    {
        var startDate = booking.Date.ToDateTime(booking.TimeSlot.Start);
        var endDate = booking.Date.ToDateTime(booking.TimeSlot.End);

        if (tz.IsInvalidTime(startDate) || tz.IsInvalidTime(endDate))
        {
            logger.LogWarning("Invalid time detected for booking {BookingId} in time zone {TimeZoneId}", booking.Id, tz.Id);
            return Result<BookingDto>.Failure(Error.BadRequest("Horário inválido para o fuso horário selecionado (possível transição de horário de verão)."));
        }

        DateTime startUtc;
        if (tz.IsAmbiguousTime(startDate))
        {
            var offsets = tz.GetAmbiguousTimeOffsets(startDate);
            logger.LogInformation("Ambiguous start time detected for booking {BookingId}. Choosing the offset {Offset} (before transition).", booking.Id, offsets[0]);
            startUtc = new DateTimeOffset(startDate, offsets[0]).UtcDateTime;
        }
        else
        {
            startUtc = TimeZoneInfo.ConvertTimeToUtc(startDate, tz);
        }

        DateTime endUtc;
        if (tz.IsAmbiguousTime(endDate))
        {
            var offsets = tz.GetAmbiguousTimeOffsets(endDate);
            logger.LogInformation("Ambiguous end time detected for booking {BookingId}. Choosing the offset {Offset} (before transition).", booking.Id, offsets[0]);
            endUtc = new DateTimeOffset(endDate, offsets[0]).UtcDateTime;
        }
        else
        {
            endUtc = TimeZoneInfo.ConvertTimeToUtc(endDate, tz);
        }

        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(startUtc, tz);
        var endLocal = TimeZoneInfo.ConvertTimeFromUtc(endUtc, tz);

        return Result<BookingDto>.Success(new BookingDto(
            booking.Id,
            booking.ProviderId,
            booking.ClientId,
            booking.ServiceId,
            new DateTimeOffset(startLocal, tz.GetUtcOffset(startUtc)),
            new DateTimeOffset(endLocal, tz.GetUtcOffset(endUtc)),
            booking.Status,
            booking.RejectionReason,
            booking.CancellationReason));
    }
}
