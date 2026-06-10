using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.Bookings;

public class BookingsModuleApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Bookings;

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingExists_ReturnsBooking()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        
        using (var scope = Services.CreateScope())
        {
            var bookingsDb = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
            var booking = new Booking(bookingId, providerId, clientId, serviceId, DateOnly.FromDateTime(DateTime.UtcNow), 
                TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)), 
                EBookingStatus.Pending, 1);
            bookingsDb.Bookings.Add(booking);
            await bookingsDb.SaveChangesAsync();
        }

        // Act
        var result = default(Result<BookingDto>);
        using (var scope = Services.CreateScope())
        {
            var bookingsApi = scope.ServiceProvider.GetRequiredService<IBookingsModuleApi>();
            result = await bookingsApi.GetBookingByIdAsync(bookingId);
        }

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(bookingId);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenNotFound_ReturnsSuccessWithNull()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        // Act
        var result = default(Result<BookingDto>);
        using (var scope = Services.CreateScope())
        {
            var bookingsApi = scope.ServiceProvider.GetRequiredService<IBookingsModuleApi>();
            result = await bookingsApi.GetBookingByIdAsync(bookingId);
        }

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetProviderBookingsAsync_WhenPeriodHasBookings_ReturnsMappedDtos()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        
        using (var scope = Services.CreateScope())
        {
            var bookingsDb = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
            var booking = new Booking(Guid.NewGuid(), providerId, Guid.NewGuid(), Guid.NewGuid(), date, 
                TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)), 
                EBookingStatus.Confirmed, 1);
            bookingsDb.Bookings.Add(booking);
            await bookingsDb.SaveChangesAsync();
        }

        // Act
        var result = default(Result<IReadOnlyList<BookingDto>>);
        using (var scope = Services.CreateScope())
        {
            var bookingsApi = scope.ServiceProvider.GetRequiredService<IBookingsModuleApi>();
            
            var start = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var end = new DateTimeOffset(date.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            result = await bookingsApi.GetProviderBookingsAsync(providerId, start, end);
        }

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value![0].ProviderId.Should().Be(providerId);
        result.Value![0].Status.Should().Be(EBookingStatus.Confirmed);
    }

    [Fact]
    public async Task HasCompletedBookingAsync_WhenCompletedBookingExists_ReturnsTrue()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        
        using (var scope = Services.CreateScope())
        {
            var bookingsDb = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
            var booking = new Booking(Guid.NewGuid(), providerId, clientId, Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 
                TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)), 
                EBookingStatus.Completed, 1);
            bookingsDb.Bookings.Add(booking);
            await bookingsDb.SaveChangesAsync();
        }

        // Act
        var result = default(Result<bool>);
        using (var scope = Services.CreateScope())
        {
            var bookingsApi = scope.ServiceProvider.GetRequiredService<IBookingsModuleApi>();
            result = await bookingsApi.HasCompletedBookingAsync(clientId, providerId);
        }

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task HasCompletedBookingAsync_WhenOnlyPendingOrCancelled_ReturnsFalse()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        
        using (var scope = Services.CreateScope())
        {
            var bookingsDb = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
            var b1 = new Booking(Guid.NewGuid(), providerId, clientId, Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 
                TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)), 
                EBookingStatus.Pending, 1);
            var b2 = new Booking(Guid.NewGuid(), providerId, clientId, Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 
                TimeSlot.Create(new TimeOnly(11, 0), new TimeOnly(12, 0)), 
                EBookingStatus.Cancelled, 1);
            
            bookingsDb.Bookings.AddRange(b1, b2);
            await bookingsDb.SaveChangesAsync();
        }

        // Act
        var result = default(Result<bool>);
        using (var scope = Services.CreateScope())
        {
            var bookingsApi = scope.ServiceProvider.GetRequiredService<IBookingsModuleApi>();
            result = await bookingsApi.HasCompletedBookingAsync(clientId, providerId);
        }

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task GetProviderBookingsAsync_WhenMoreThanOneHundredBookings_ShouldReturnAll()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var bookingsCount = 101;
        
        using (var scope = Services.CreateScope())
        {
            var bookingsDb = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
            for (int i = 0; i < bookingsCount; i++)
            {
                var booking = new Booking(Guid.NewGuid(), providerId, Guid.NewGuid(), Guid.NewGuid(), date, 
                    TimeSlot.Create(new TimeOnly(0, 0).AddMinutes(i), new TimeOnly(0, 1).AddMinutes(i)), 
                    EBookingStatus.Confirmed, 1);
                bookingsDb.Bookings.Add(booking);
            }
            await bookingsDb.SaveChangesAsync();
        }

        // Act
        var result = default(Result<IReadOnlyList<BookingDto>>);
        using (var scope = Services.CreateScope())
        {
            var bookingsApi = scope.ServiceProvider.GetRequiredService<IBookingsModuleApi>();
            
            var start = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var end = new DateTimeOffset(date.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            result = await bookingsApi.GetProviderBookingsAsync(providerId, start, end);
        }

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(bookingsCount);
    }
}
