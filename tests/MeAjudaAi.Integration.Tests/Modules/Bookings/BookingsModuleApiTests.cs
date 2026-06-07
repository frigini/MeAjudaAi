using FluentAssertions;
using MeAjudaAi.Contracts.Modules.Bookings;
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
                TimeSlot.Create(TimeOnly.FromDateTime(DateTime.UtcNow), TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1))), 
                EBookingStatus.Pending, 1);
            bookingsDb.Bookings.Add(booking);
            await bookingsDb.SaveChangesAsync();
        }

        using (var scope = Services.CreateScope())
        {
            var bookingsApi = scope.ServiceProvider.GetRequiredService<IBookingsModuleApi>();
            
            // Act
            var result = await bookingsApi.GetBookingByIdAsync(bookingId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(bookingId);
        }
    }
}
