using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Bookings;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.ModuleApi;

[ModuleApi("Bookings", "1.0")]
public sealed class BookingsModuleApi(
    IBookingQueries bookingQueries,
    ILogger<BookingsModuleApi> logger) : IBookingsModuleApi
{
    public string ModuleName => "Bookings";
    public string ApiVersion => "1.0";

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(true);
    }

    public async Task<Result<BookingDto?>> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        try
        {
            var booking = await bookingQueries.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null) return Result<BookingDto?>.Success(null);

            return Result<BookingDto?>.Success(MapToDto(booking));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting booking {BookingId}", bookingId);
            return Result<BookingDto?>.Failure("Error retrieving booking data.");
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

    public async Task<Result<IReadOnlyList<BookingDto>>> GetProviderBookingsAsync(Guid providerId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromDate = DateOnly.FromDateTime(start.Date);
            var toDate = DateOnly.FromDateTime(end.Date);
            var allBookings = new List<MeAjudaAi.Modules.Bookings.Domain.Entities.Booking>();
            int page = 1;
            const int pageSize = 1000;
            bool hasMore = true;

            while (hasMore)
            {
                var (items, _) = await bookingQueries.GetByProviderIdPagedAsync(providerId, fromDate, toDate, page, pageSize, cancellationToken);
                allBookings.AddRange(items);
                hasMore = items.Count == pageSize;
                page++;
            }
            
            var dtos = allBookings.Select(MapToDto).ToList();
            return Result<IReadOnlyList<BookingDto>>.Success(dtos);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting bookings for provider {ProviderId}", providerId);
            return Result<IReadOnlyList<BookingDto>>.Failure("Error retrieving bookings.");
        }
    }

    private static BookingDto MapToDto(MeAjudaAi.Modules.Bookings.Domain.Entities.Booking booking)
    {
        var startTime = booking.Date.ToDateTime(booking.TimeSlot.Start);
        var endTime = booking.Date.ToDateTime(booking.TimeSlot.End);
        
        var startOffset = new DateTimeOffset(DateTime.SpecifyKind(startTime, DateTimeKind.Utc), TimeSpan.Zero);
        var endOffset = new DateTimeOffset(DateTime.SpecifyKind(endTime, DateTimeKind.Utc), TimeSpan.Zero);
        
        return new BookingDto(
            booking.Id,
            booking.ProviderId,
            booking.ClientId,
            booking.ServiceId,
            startOffset,
            endOffset,
            booking.Status,
            booking.RejectionReason,
            booking.CancellationReason);
    }
}
