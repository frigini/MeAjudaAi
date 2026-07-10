using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using System.Globalization;
using System.Net.Http.Json;

namespace MeAjudaAi.Integration.Tests.Modules.Bookings.Api;

public class BookingsApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.All;

    [Fact]
    public async Task CreateBooking_ShouldReturnCreated_WhenRequestIsValid()
    {
        var providerId = await CreateTestProviderViaDbAsync();
        await CreateTestScheduleViaDbAsync(providerId);
        var serviceId = await CreateTestServiceViaDbAsync();
        await LinkServiceToProviderViaDbAsync(providerId, serviceId, "Test Service");

        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var start = tomorrow.ToDateTime(new TimeOnly(10, 0));
        var request = new CreateBookingRequestDto(
            providerId,
            serviceId,
            new DateTimeOffset(start, TimeSpan.Zero),
            new DateTimeOffset(start.AddHours(1), TimeSpan.Zero));

        AuthConfig.ConfigureRegularUser(Guid.NewGuid().ToString());

        var response = await Client.PostAsJsonAsync("/api/v1/bookings", request);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var error = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.Created, $"Error detail: {error}");
        }

        var result = await ReadJsonAsync<ModuleBookingDto>(response.Content);
        result.Should().NotBeNull();
        result!.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task GetProviderAvailability_ShouldReturnSlots()
    {
        var providerId = await CreateTestProviderViaDbAsync();
        await CreateTestScheduleViaDbAsync(providerId);
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var dateString = tomorrow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        AuthConfig.ConfigureRegularUser(Guid.NewGuid().ToString());

        var response = await Client.GetAsync($"/api/v1/bookings/availability/{providerId}?date={dateString}");

        response.StatusCode.Should().Be(HttpStatusCode.OK, $"server returned: {await response.Content.ReadAsStringAsync()}");
        var availability = await ReadJsonAsync<AvailabilityDto>(response.Content);
        availability.Should().NotBeNull();
        availability!.Slots.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SetProviderSchedule_ShouldReturnNoContent_WhenRequestIsValid()
    {
        var providerId = await CreateTestProviderViaDbAsync();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var availabilities = new[]
        {
            new AvailabilityDto(
                DayOfWeek.Monday,
                new List<AvailableSlotDto> {
                    new(new DateTimeOffset(tomorrow.ToDateTime(new TimeOnly(8, 0)), TimeSpan.Zero), new DateTimeOffset(tomorrow.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero)),
                    new(new DateTimeOffset(tomorrow.ToDateTime(new TimeOnly(13, 0)), TimeSpan.Zero), new DateTimeOffset(tomorrow.ToDateTime(new TimeOnly(17, 0)), TimeSpan.Zero))
                })
        };
        var request = new SetProviderScheduleRequestDto(providerId, availabilities);
        AuthConfig.ConfigureProvider(providerId, Guid.NewGuid().ToString());

        var response = await Client.PostAsJsonAsync("/api/v1/bookings/schedule", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent, $"server returned: {await response.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task GetProviderBookings_ShouldReturnOk_WhenAuthorized()
    {
        var providerId = await CreateTestProviderViaDbAsync();
        var otherProviderId = await CreateTestProviderViaDbAsync();
        var serviceId = await CreateTestServiceViaDbAsync();
        await LinkServiceToProviderViaDbAsync(otherProviderId, serviceId, "Other Service");
        
        // Criar agendamento para o OUTRO provedor
        await CreateTestBookingAsync(otherProviderId, Guid.NewGuid(), serviceId);
        
        AuthConfig.ConfigureProvider(providerId, Guid.NewGuid().ToString());

        var response = await Client.GetAsync($"/api/v1/bookings/provider/{providerId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await ReadJsonAsync<PagedResult<ModuleBookingDto>>(response.Content);
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty(); // Não deve ver agendamentos de outros provedores
    }

    [Fact]
    public async Task GetBookingById_ShouldReturnOk_WhenBookingExists()
    {
        var providerId = await CreateTestProviderViaDbAsync();
        await CreateTestScheduleViaDbAsync(providerId);
        var serviceId = await CreateTestServiceViaDbAsync();
        await LinkServiceToProviderViaDbAsync(providerId, serviceId, "Test Service");

        var clientId = Guid.NewGuid();
        var bookingId = await CreateTestBookingAsync(providerId, clientId, serviceId);

        AuthConfig.ConfigureRegularUser(clientId.ToString());

        var response = await Client.GetAsync($"/api/v1/bookings/{bookingId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var booking = await ReadJsonAsync<ModuleBookingDto>(response.Content);
        booking.Should().NotBeNull();
        booking!.Id.Should().Be(bookingId);
    }

    [Fact]
    public async Task CancelBooking_ShouldReturnNoContent_WhenAuthorized()
    {
        var providerId = await CreateTestProviderViaDbAsync();
        await CreateTestScheduleViaDbAsync(providerId);
        var serviceId = await CreateTestServiceViaDbAsync();
        await LinkServiceToProviderViaDbAsync(providerId, serviceId, "Test Service");

        var clientId = Guid.NewGuid();
        var bookingId = await CreateTestBookingAsync(providerId, clientId, serviceId);

        AuthConfig.ConfigureRegularUser(clientId.ToString());

        var cancelRequest = new CancelBookingRequestDto("Test Cancel");
        var response = await Client.PutAsJsonAsync($"/api/v1/bookings/{bookingId}/cancel", cancelRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CancelBooking_ShouldReturnNoContent_WhenClientIsNotALinkedProvider()
    {
        // 1. Arrange: Criar provedor, serviço e agendamento
        var providerId = await CreateTestProviderViaDbAsync();
        await CreateTestScheduleViaDbAsync(providerId);
        var serviceId = await CreateTestServiceViaDbAsync();
        await LinkServiceToProviderViaDbAsync(providerId, serviceId, "Test Service");

        var clientId = Guid.NewGuid();
        var bookingId = await CreateTestBookingAsync(providerId, clientId, serviceId);

        // 2. Act: Configurar como usuário regular (sem ProviderId vinculado) e tentar cancelar
        AuthConfig.ConfigureRegularUser(clientId.ToString());

        var cancelRequest = new CancelBookingRequestDto("Client cancelling own booking");
        var response = await Client.PutAsJsonAsync($"/api/v1/bookings/{bookingId}/cancel", cancelRequest);

        // 3. Assert: Deve permitir cancelamento
        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            var error = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.NoContent, $"Error detail: {error}");
        }
    }

    [Fact]
    public async Task GetMyBookings_ShouldReturnOk_WhenAuthorized()
    {
        var providerId = await CreateTestProviderViaDbAsync();
        await CreateTestScheduleViaDbAsync(providerId);
        var serviceId = await CreateTestServiceViaDbAsync();
        await LinkServiceToProviderViaDbAsync(providerId, serviceId, "Test Service");

        var clientId = Guid.NewGuid();
        await CreateTestBookingAsync(providerId, clientId, serviceId);

        AuthConfig.ConfigureRegularUser(clientId.ToString());

        var response = await Client.GetAsync("/api/v1/bookings/my");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await ReadJsonAsync<PagedResult<ModuleBookingDto>>(response.Content);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetMyBookings_WithoutAuth_ShouldReturn401()
    {
        AuthConfig.ClearConfiguration();

        var response = await Client.GetAsync("/api/v1/bookings/my");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyBookings_NoBookings_ShouldReturn200WithEmpty()
    {
        var clientId = Guid.NewGuid();
        AuthConfig.ConfigureRegularUser(clientId.ToString());

        var response = await Client.GetAsync("/api/v1/bookings/my");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await ReadJsonAsync<PagedResult<ModuleBookingDto>>(response.Content);
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task GetMyBookings_WithPagination_ShouldRespectPageSize()
    {
        var providerId = await CreateTestProviderViaDbAsync();
        await CreateTestScheduleViaDbAsync(providerId);
        var serviceId = await CreateTestServiceViaDbAsync();
        await LinkServiceToProviderViaDbAsync(providerId, serviceId, "Test Service");

        var clientId = Guid.NewGuid();
        for (int i = 0; i < 5; i++)
            await CreateTestBookingAsync(providerId, clientId, serviceId);

        AuthConfig.ConfigureRegularUser(clientId.ToString());

        var response = await Client.GetAsync("/api/v1/bookings/my?page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await ReadJsonAsync<PagedResult<ModuleBookingDto>>(response.Content);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.PageSize.Should().Be(2);
        result.TotalItems.Should().Be(5);
    }

    [Fact]
    public async Task GetMyBookings_OnlyReturnsOwnBookings_ShouldExcludeOtherClients()
    {
        var providerId = await CreateTestProviderViaDbAsync();
        await CreateTestScheduleViaDbAsync(providerId);
        var serviceId = await CreateTestServiceViaDbAsync();
        await LinkServiceToProviderViaDbAsync(providerId, serviceId, "Test Service");

        var myClientId = Guid.NewGuid();
        var otherClientId = Guid.NewGuid();
        await CreateTestBookingAsync(providerId, myClientId, serviceId);
        await CreateTestBookingAsync(providerId, otherClientId, serviceId);

        AuthConfig.ConfigureRegularUser(myClientId.ToString());

        var response = await Client.GetAsync("/api/v1/bookings/my");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await ReadJsonAsync<PagedResult<ModuleBookingDto>>(response.Content);
        result!.Items.Should().HaveCount(1);
        result.Items.First().ClientId.Should().Be(myClientId);
    }

    private async Task<Guid> CreateTestBookingAsync(Guid providerId, Guid clientId, Guid serviceId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var slot = TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0));
        var booking = Booking.Create(providerId, clientId, serviceId, tomorrow, slot);
        
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
        
        return booking.Id;
    }
}
