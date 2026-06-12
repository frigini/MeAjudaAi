using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Bookings;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.ModuleApi;

[ModuleApi(ModuleNames.Bookings)]
public sealed class BookingsModuleApi(
    IBookingQueries bookingQueries,
    ILogger<BookingsModuleApi> logger) : IBookingsModuleApi
{
    public string ModuleName => ModuleNames.Bookings;
    public string ApiVersion => "1.0";

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(true);
    }

    public async Task<Result<ModuleBookingDto?>> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        try
        {
            var booking = await bookingQueries.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null) return Result<ModuleBookingDto?>.Success(null);

            return Result<ModuleBookingDto?>.Success(MapToDto(booking));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting booking {BookingId}", bookingId);
            return Result<ModuleBookingDto?>.Failure("Error retrieving booking data.");
        }
    }

    public async Task<Result<bool>> HasCompletedBookingAsync(Guid clientId, Guid providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var hasBooking = await bookingQueries.HasCompletedBookingAsync(clientId, providerId, cancellationToken);
            return Result<bool>.Success(hasBooking);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking completed booking for client {ClientId} and provider {ProviderId}", clientId, providerId);
            return Result<bool>.Failure("Error checking booking history.");
        }
    }

    public async Task<Result<IReadOnlyList<ModuleBookingDto>>> GetProviderBookingsAsync(Guid providerId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromDate = DateOnly.FromDateTime(start.Date);
            var toDate = DateOnly.FromDateTime(end.Date);
            
            var bookings = await bookingQueries.GetByProviderAndPeriodAsync(providerId, fromDate, toDate, cancellationToken);
            
            var dtos = bookings.Select(MapToDto).ToList();
            return Result<IReadOnlyList<ModuleBookingDto>>.Success(dtos);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting bookings for provider {ProviderId}", providerId);
            return Result<IReadOnlyList<ModuleBookingDto>>.Failure("Error retrieving bookings.");
        }
    }

    private static ModuleBookingDto MapToDto(Booking booking)
    {
        return new ModuleBookingDto(
            booking.Id,
            booking.ProviderId,
            booking.ClientId,
            booking.ServiceId,
            booking.Date,
            booking.TimeSlot.Start,
            booking.TimeSlot.End,
            booking.Status,
            booking.RejectionReason,
            booking.CancellationReason);
    }
}
