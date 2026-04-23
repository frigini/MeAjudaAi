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
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
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
        var serviceId = await CreateTestServiceAsync(providerId);

        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var start = tomorrow.ToDateTime(new TimeOnly(10, 0));
        var request = new CreateBookingRequest(
            providerId,
            serviceId,
            new DateTimeOffset(start, TimeSpan.Zero),
            new DateTimeOffset(start.AddHours(1), TimeSpan.Zero));

        AuthConfig.ConfigureRegularUser(Guid.NewGuid().ToString());
        Client.AsTestInstance();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/bookings", request);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var error = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.Created, $"Error detail: {error}");
        }

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
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var dateString = tomorrow.ToString("yyyy-MM-dd");

        AuthConfig.ConfigureRegularUser("client-id");
        Client.AsTestInstance();

        // Act
        var response = await Client.GetAsync($"/api/v1/bookings/availability/{providerId}?date={dateString}");

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed with status {response.StatusCode} and content: {error}");
        }

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

    private async Task<Guid> CreateTestServiceAsync(Guid providerId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext>();
        
        var category = ServiceCategory.Create("Test Category", null, 1);
        context.ServiceCategories.Add(category);
        await context.SaveChangesAsync();

        var service = Service.Create(category.Id, "Test Service", "Description");
        
        context.Services.Add(service);
        await context.SaveChangesAsync();

        return service.Id.Value;
    }

    private async Task CreateTestScheduleAsync(Guid providerId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        
        var schedule = ProviderSchedule.Create(providerId, "UTC");
        
        // Adiciona para todos os dias da semana para facilitar o teste
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var slots = new[] { TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(18, 0)) };
            schedule.SetAvailability(Availability.Create(day, slots));
        }
        
        context.ProviderSchedules.Add(schedule);
        await context.SaveChangesAsync();
    }
}
