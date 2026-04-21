using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Bookings.API.Endpoints.Public;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Bookings;

public class BookingsApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.All;

    [Fact]
    public async Task CreateBooking_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var providerId = await CreateTestProviderAsync();
        await CreateTestScheduleAsync(providerId);
        
        var serviceId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(10);
        var request = new CreateBookingRequest(
            providerId,
            serviceId,
            new DateTimeOffset(start, TimeSpan.Zero),
            new DateTimeOffset(start.AddHours(1), TimeSpan.Zero));

        await AuthConfig.AuthenticateAsClientAsync(Client);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/bookings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var booking = await response.Content.ReadFromJsonAsync<BookingDto>();
        booking.Should().NotBeNull();
        booking!.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task GetProviderAvailability_ShouldReturnSlots()
    {
        // Arrange
        var providerId = await CreateTestProviderAsync();
        await CreateTestScheduleAsync(providerId);
        var date = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

        // Act
        var response = await Client.GetAsync($"/api/v1/bookings/availability/{providerId}?date={date}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var availability = await response.Content.ReadFromJsonAsync<AvailabilityDto>();
        availability.Should().NotBeNull();
        availability!.Slots.Should().NotBeEmpty();
    }

    private async Task<Guid> CreateTestProviderAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        
        var provider = Provider.Create(Guid.NewGuid(), "Test Provider", "test-provider", "12345678901", "test@test.com");
        context.Providers.Add(provider);
        await context.SaveChangesAsync();
        
        return provider.Id;
    }

    private async Task CreateTestScheduleAsync(Guid providerId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        
        var schedule = ProviderSchedule.Create(providerId, "UTC");
        var slots = new[] { TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(18, 0)) };
        // Adiciona para todos os dias da semana para facilitar o teste
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            schedule.SetAvailability(Availability.Create(day, slots));
        }
        
        context.ProviderSchedules.Add(schedule);
        await context.SaveChangesAsync();
    }
}
