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
            // Busca simplificada para verificar se existe algum booking completed
            // PERF: Em uma implementação real, deveríamos ter uma query específica "Any" no repositório.
            var (items, _) = await bookingQueries.GetByClientIdPagedAsync(clientId, null, null, 1, 100, cancellationToken);
            
            var hasCompleted = items.Any(b => b.ProviderId == providerId && b.Status == MeAjudaAi.Contracts.Modules.Bookings.Enums.EBookingStatus.Completed);
            
            return Result<bool>.Success(hasCompleted);
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

            var (items, _) = await bookingQueries.GetByProviderIdPagedAsync(providerId, fromDate, toDate, 1, 1000, cancellationToken);
            
            var dtos = items.Select(MapToDto).ToList();
            return Result<IReadOnlyList<BookingDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting bookings for provider {ProviderId}", providerId);
            return Result<IReadOnlyList<BookingDto>>.Failure("Error retrieving bookings.");
        }
    }

    private static BookingDto MapToDto(MeAjudaAi.Modules.Bookings.Domain.Entities.Booking booking)
    {
        // Precisamos converter os horários de volta para DateTimeOffset. 
        // Como o domínio armazena Date + TimeSlot, precisamos de um fuso horário ou assumir UTC se não disponível.
        // Simplificação: usando a data diretamente.
        return new BookingDto(
            booking.Id,
            booking.ProviderId,
            booking.ClientId,
            booking.ServiceId,
            new DateTimeOffset(booking.Date.ToDateTime(booking.TimeSlot.Start)),
            new DateTimeOffset(booking.Date.ToDateTime(booking.TimeSlot.End)),
            booking.Status,
            booking.RejectionReason,
            booking.CancellationReason);
    }
}
