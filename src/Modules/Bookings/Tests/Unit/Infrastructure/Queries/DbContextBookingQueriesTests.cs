using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Queries;
using MeAjudaAi.Modules.Bookings.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Integration")]
[Trait("Module", "Bookings")]
[Trait("Layer", "Infrastructure")]
[Collection("BookingsIntegrationTests")]
public class DbContextBookingQueriesTests : BookingsIntegrationTestBase
{
    private DbContextBookingQueries _queries = null!;

    protected override async Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        await base.OnModuleInitializeAsync(serviceProvider);
        _queries = new DbContextBookingQueries(serviceProvider.GetRequiredService<BookingsDbContext>());
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingBooking_ShouldReturnBooking()
    {
        // Arrange
        var booking = new BookingBuilder()
            .WithProviderId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .WithStatus(EBookingStatus.Pending)
            .Build();
        using (var scope = CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await _queries.GetByIdAsync(booking.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(booking.Id);
    }

    [Fact]
    public async Task HasCompletedBookingAsync_WhenCompletedBookingExists_ShouldReturnTrue()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var booking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(clientId)
            .WithServiceId(Guid.NewGuid())
            .WithDate(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .AsCompleted()
            .Build();
        using (var scope = CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await _queries.HasCompletedBookingAsync(clientId, providerId);

        // Assert
        result.Should().BeTrue();
    }
}