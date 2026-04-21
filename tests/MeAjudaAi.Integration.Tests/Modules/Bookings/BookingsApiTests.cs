using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Bookings.API.Endpoints.Public;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.Extensions;
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

        AuthConfig.ConfigureRegularUser("client-id");
        Client.AsUser();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/bookings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await ReadJsonAsync<BookingDto>(response.Content);
        result.Should().NotBeNull();
        result!.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task GetProviderAvailability_ShouldReturnSlots()
    {
        // Arrange
        var providerId = await CreateTestProviderAsync();
        await CreateTestScheduleAsync(providerId);
        var date = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

        AuthConfig.ConfigureRegularUser("client-id");
        Client.AsUser();

        // Act
        var response = await Client.GetAsync($"/api/v1/bookings/availability/{providerId}?date={date}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var availability = await ReadJsonAsync<AvailabilityDto>(response.Content);
        availability.Should().NotBeNull();
        availability!.Slots.Should().NotBeEmpty();
    }

    private async Task<Guid> CreateTestProviderAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        
        var contactInfo = new ContactInfo("test@test.com", "12345678901");
        var businessProfile = new BusinessProfile("Test Provider", contactInfo, null);
        var provider = new MeAjudaAi.Modules.Providers.Domain.Entities.Provider(
            Guid.NewGuid(), 
            "Test Provider", 
            EProviderType.Individual, 
            businessProfile);

        context.Providers.Add(provider);
        await context.SaveChangesAsync();
        
        return provider.Id.Value;
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
