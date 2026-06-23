using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Bookings;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Bookings.Application.ModuleApi;

/// <summary>
/// Implementação da API pública do módulo Bookings para comunicação entre módulos.
/// </summary>
[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class BookingsModuleApi(
    IBookingQueries bookingQueries) : IBookingsModuleApi
{
    private static class ModuleMetadata
    {
        public const string Name = ModuleNames.Bookings;
        public const string Version = "1.0";
    }

    public string ModuleName => ModuleMetadata.Name;
    public string ApiVersion => ModuleMetadata.Version;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await bookingQueries.CanConnectAsync(cancellationToken);
    }

    public async Task<Result<ModuleBookingDto?>> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await bookingQueries.GetByIdAsync(bookingId, cancellationToken);
        if (booking == null) return Result<ModuleBookingDto?>.Success(null);

        return Result<ModuleBookingDto?>.Success(MapToDto(booking));
    }

    public async Task<Result<bool>> HasCompletedBookingAsync(Guid clientId, Guid providerId, CancellationToken cancellationToken = default)
    {
        var hasBooking = await bookingQueries.HasCompletedBookingAsync(clientId, providerId, cancellationToken);
        return Result<bool>.Success(hasBooking);
    }

    public async Task<Result<IReadOnlyList<ModuleBookingDto>>> GetProviderBookingsAsync(Guid providerId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
    {
        var fromDate = DateOnly.FromDateTime(start.Date);
        var toDate = DateOnly.FromDateTime(end.Date);

        var bookings = await bookingQueries.GetByProviderAndPeriodAsync(providerId, fromDate, toDate, cancellationToken);

        var dtos = bookings.Select(MapToDto).ToList();
        return Result<IReadOnlyList<ModuleBookingDto>>.Success(dtos);
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
