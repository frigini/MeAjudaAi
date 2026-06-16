using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Services;

public static class TimeZoneResolver
{
    public static TimeZoneInfo? ResolveTimeZone(string? timeZoneId, ILogger logger, bool allowFallback = true)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException ex)
            {
                if (allowFallback)
                {
                    logger.LogWarning(ex, "Failed to resolve time zone {TimeZoneId}. Falling back.", timeZoneId);
                }
                else
                {
                    logger.LogError(ex, "Failed to resolve time zone {TimeZoneId}. Strict resolution requested.", timeZoneId);
                    return null;
                }
            }
            catch (InvalidTimeZoneException ex)
            {
                if (allowFallback)
                {
                    logger.LogWarning(ex, "Failed to resolve time zone {TimeZoneId}. Falling back.", timeZoneId);
                }
                else
                {
                    logger.LogError(ex, "Failed to resolve time zone {TimeZoneId}. Strict resolution requested.", timeZoneId);
                    return null;
                }
            }
            catch (System.Security.SecurityException ex)
            {
                // Problemas de segurança são esperados em alguns ambientes — trate-os da mesma forma
                if (allowFallback)
                {
                    logger.LogWarning(ex, "Failed to resolve time zone {TimeZoneId} due to security. Falling back.", timeZoneId);
                }
                else
                {
                    logger.LogError(ex, "Failed to resolve time zone {TimeZoneId} due to security. Strict resolution requested.", timeZoneId);
                    return null;
                }
            }
        }

        if (!allowFallback)
        {
            logger.LogWarning("Strict time zone resolution failed for {TimeZoneId}. No fallback allowed.", timeZoneId);
            return null;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
        catch (TimeZoneNotFoundException ex)
        {
            logger.LogWarning(ex, "Failed to resolve Windows Brazil time zone. Trying IANA.");
        }
        catch (InvalidTimeZoneException ex)
        {
            logger.LogWarning(ex, "Failed to resolve Windows Brazil time zone. Trying IANA.");
        }
        catch (System.Security.SecurityException ex)
        {
            logger.LogWarning(ex, "Failed to resolve Windows Brazil time zone due to security. Trying IANA.");
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneConstants.DefaultTimeZoneId);
        }
        catch (TimeZoneNotFoundException exIana)
        {
            logger.LogWarning(exIana, "Failed to resolve IANA Brazil time zone. Using local/UTC.");
        }
        catch (InvalidTimeZoneException exIana)
        {
            logger.LogWarning(exIana, "Failed to resolve IANA Brazil time zone. Using local/UTC.");
        }
        catch (System.Security.SecurityException exIana)
        {
            logger.LogWarning(exIana, "Failed to resolve IANA Brazil time zone due to security. Using local/UTC.");
        }

        try
        {
            return TimeZoneInfo.Local;
        }
        catch (System.Security.SecurityException ex)
        {
            logger.LogWarning(ex, "Failed to access local time zone due to security. Using UTC.");
            return TimeZoneInfo.Utc;
        }
    }

    public static Result<ModuleBookingDto> CreateValidatedBookingDto(Booking booking, TimeZoneInfo tz, ILogger logger)
    {
        var startDate = booking.Date.ToDateTime(booking.TimeSlot.Start);
        var endDate = booking.Date.ToDateTime(booking.TimeSlot.End);

        if (tz.IsInvalidTime(startDate) || tz.IsInvalidTime(endDate))
        {
            logger.LogWarning("Invalid time detected for booking {BookingId} in time zone {TimeZoneId}", booking.Id, tz.Id);
            return Result<ModuleBookingDto>.Failure(Error.BadRequest("Horário inválido para o fuso horário selecionado (possível transição de horário de verão)."));
        }

        return Result<ModuleBookingDto>.Success(new ModuleBookingDto(
            booking.Id,
            booking.ProviderId,
            booking.ClientId,
            booking.ServiceId,
            booking.Date,
            booking.TimeSlot.Start,
            booking.TimeSlot.End,
            booking.Status,
            booking.RejectionReason,
            booking.CancellationReason));
    }
}
