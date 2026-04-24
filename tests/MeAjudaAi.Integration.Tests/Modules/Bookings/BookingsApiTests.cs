using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.Contracts.Models;
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
        var providerId = await CreateTestProviderAsync();
        await CreateTestScheduleAsync(providerId);
        var serviceId = await CreateTestServiceAsync();

        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var start = tomorrow.ToDateTime(new TimeOnly(10, 0));
        var request = new CreateBookingRequest(
            providerId,
            serviceId,
            new DateTimeOffset(start, TimeSpan.Zero),
            new DateTimeOffset(start.AddHours(1), TimeSpan.Zero));

        AuthConfig.ConfigureRegularUser(Guid.NewGuid().ToString());
        Client.AsTestInstance();

        var response = await Client.PostAsJsonAsync("/api/v1/bookings", request);

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
        var providerId = await CreateTestProviderAsync();
        await CreateTestScheduleAsync(providerId);
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var dateString = tomorrow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        AuthConfig.ConfigureRegularUser("client-id");
        Client.AsTestInstance();

        var response = await Client.GetAsync($"/api/v1/bookings/availability/{providerId}?date={dateString}");

        response.StatusCode.Should().Be(HttpStatusCode.OK, $"server returned: {await response.Content.ReadAsStringAsync()}");
        var availability = await ReadJsonAsync<AvailabilityDto>(response.Content);
        availability.Should().NotBeNull();
        availability!.Slots.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SetProviderSchedule_ShouldReturnNoContent_WhenRequestIsValid()
    {
        var providerId = await CreateTestProviderAsync();
        var availabilities = new[]
        {
            new ProviderScheduleDto(
                DayOfWeek.Monday,
                new[] { 
                    new TimeSlotDto(new TimeOnly(8, 0), new TimeOnly(12, 0)), 
                    new TimeSlotDto(new TimeOnly(13, 0), new TimeOnly(17, 0)) 
                })
        };
        var request = new SetProviderScheduleRequest(providerId, availabilities);

        AuthConfig.ConfigureProvider(providerId, "provider-user-id");
        Client.AsTestInstance();

        var response = await Client.PostAsJsonAsync("/api/v1/bookings/schedule", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent, $"server returned: {await response.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task GetProviderBookings_ShouldReturnOk_WhenAuthorized()
    {
        var providerId = await CreateTestProviderAsync();
        
        AuthConfig.ConfigureProvider(providerId, "provider-user-id");
        Client.AsTestInstance();

        var response = await Client.GetAsync($"/api/v1/bookings/provider/{providerId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await ReadJsonAsync<PagedResult<BookingDto>>(response.Content);
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty(); // No bookings created yet
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

    private async Task<Guid> CreateTestServiceAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
        
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
        
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var slots = new[] { TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(18, 0)) };
            schedule.SetAvailability(Availability.Create(day, slots));
        }
        
        context.ProviderSchedules.Add(schedule);
        await context.SaveChangesAsync();
    }
}