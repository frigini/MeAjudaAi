using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MeAjudaAi.Integration.Tests.Base;

public abstract partial class BaseApiTest
{
    #region Provider Helpers

    protected async Task<Guid> CreateTestProviderViaApiAsync(
        Guid userId, string name, string? city = null, string? state = null)
    {
        city ??= "Muriaé";
        state ??= "MG";
        AuthConfig.ConfigureAdmin();

        var request = new
        {
            userId = userId,
            name = name,
            type = (int)EProviderType.Individual,
            businessProfile = new
            {
                legalName = name,
                description = "Test provider",
                contactInfo = new
                {
                    email = $"{Guid.NewGuid():N}@test.com",
                    phoneNumber = "+5511999999999"
                },
                primaryAddress = new
                {
                    street = "Rua Teste",
                    number = "123",
                    complement = (string?)null,
                    neighborhood = "Bairro Teste",
                    city = city,
                    state = state,
                    zipCode = "12345-678",
                    country = "Brasil"
                },
                showAddressToClient = true
            }
        };

        var response = await Client.PostAsJsonAsync(ProvidersEndpoint, request);
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(location))
            throw new InvalidOperationException("Location header not found in create provider response");

        return ExtractIdFromLocation(location);
    }

    protected async Task<Guid> CreateTestProviderViaDbAsync(string? name = null)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();

        var contactInfo = new ContactInfo("test@test.com", "12345678901");
        var businessProfile = new BusinessProfile(name ?? "Test Provider", contactInfo, null);
        var provider = new ProviderBuilder()
            .WithName(name ?? "Test Provider")
            .WithType(EProviderType.Individual)
            .WithBusinessProfile(businessProfile)
            .Build();

        context.Providers.Add(provider);
        await context.SaveChangesAsync();
        return provider.Id.Value;
    }

    protected async Task LinkServiceToProviderViaDbAsync(
        Guid providerId, Guid serviceId, string serviceName)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        var provider = await context.Providers.FindAsync(new ProviderId(providerId));
        provider!.AddService(serviceId, serviceName);
        await context.SaveChangesAsync();
    }

    #endregion

    #region User Helpers

    protected async Task<Guid> CreateTestUserViaApiAsync(
        string? username = null, string? email = null)
    {
        AuthConfig.ConfigureAdmin();
        username ??= $"user_{Guid.NewGuid():N}"[..20];
        email ??= $"{Guid.NewGuid():N}@test.com";

        var request = new
        {
            username = username,
            email = email,
            firstName = "Test",
            lastName = "User",
            password = "Password123!",
            keycloakId = Guid.NewGuid().ToString()
        };

        var response = await Client.PostAsJsonAsync("/api/v1/users", request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("data").GetProperty("id").GetGuid();
    }

    #endregion

    #region Service Helpers

    protected async Task<Guid> CreateTestServiceViaDbAsync(
        string? categoryName = null, string? serviceName = null)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();

        var category = ServiceCategory.Create(categoryName ?? "Test Category", null, 1);
        context.ServiceCategories.Add(category);
        await context.SaveChangesAsync();

        var service = Service.Create(category.Id, serviceName ?? "Test Service", "Test description");
        context.Services.Add(service);
        await context.SaveChangesAsync();
        return service.Id.Value;
    }

    #endregion

    #region Schedule Helpers

    protected async Task CreateTestScheduleViaDbAsync(
        Guid providerId, string timezone = "UTC")
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();

        var schedule = ProviderSchedule.Create(providerId, timezone);
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var slots = new[] { TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(18, 0)) };
            schedule.SetAvailability(Availability.Create(day, slots));
        }
        context.ProviderSchedules.Add(schedule);
        await context.SaveChangesAsync();
    }

    #endregion

    #region Utility

    private static Guid ExtractIdFromLocation(string location)
    {
        var lastSegment = location.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
        return Guid.Parse(lastSegment);
    }

    #endregion
}
