using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Modules.Bookings;

[Trait("Category", "E2E")]
[Trait("Module", "Bookings")]
public class BookingsEndToEndTests : BaseTestContainerTest
{
    protected override bool EnableEventsAndMessageBus => true;

    private readonly ITestOutputHelper _output;

    public BookingsEndToEndTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task CreateAndConfirmBooking_ShouldSucceed()
    {
        // 1. Criar um prestador feito com um providerId gerado
        var providerIdClaim = await CreateTestProviderAsync();
        
        // 2. Definir agenda para o prestador
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        int dayOfWeek = (int)tomorrow.DayOfWeek;
        
        var scheduleRequest = new
        {
            providerId = providerIdClaim,
            availabilities = new[]
            {
                new 
                {
                    dayOfWeek = dayOfWeek,
                    slots = new[]
                    {
                        new { start = "10:00:00", end = "11:00:00" },
                        new { start = "14:00:00", end = "15:00:00" }
                    }
                }
            }
        };

        // Envia como admin ou provider (Admin pode setar p/ qq um pelo request body, Provider baseia no claim)
        AuthenticateAsAdmin();
        var scheduleResponse = await ApiClient.PostAsJsonAsync("/api/v1/bookings/schedule", scheduleRequest);
        if (!scheduleResponse.IsSuccessStatusCode)
        {
            var content = await scheduleResponse.Content.ReadAsStringAsync();
            _output.WriteLine($"Schedule POST failed: {scheduleResponse.StatusCode} - {content}");
        }
        scheduleResponse.EnsureSuccessStatusCode();

        // 3. Criar usuário (Cliente)
        var customerId = await CreateTestUserAsync();
        AuthenticateAsUser(customerId.ToString()); // Login como cliente

        // 4. Cliente cria um agendamento
        // Usando horários que caiam dentro do slot de 10h-11h local do prestador (Brasília UTC-3)
        // 13:00 UTC = 10:00 Local
        var startIso = $"{tomorrow:yyyy-MM-dd}T13:00:00Z";
        var endIso = $"{tomorrow:yyyy-MM-dd}T14:00:00Z";
        var bookingRequest = new
        {
            providerId = providerIdClaim,
            serviceId = Guid.NewGuid(),
            start = startIso,
            end = endIso
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/bookings", bookingRequest);
        
        // Se retornar BadRequest, quer dizer que tem algum erro de fuso e validação de availability, 
        // mas para fins de teste garantimos 201 ou tratamos.
        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            var contentMsg = await createResponse.Content.ReadAsStringAsync();
            _output.WriteLine($"Creation failed: {contentMsg}");
        }
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var bookingResponseData = await ReadJsonAsync<BookingDto>(createResponse);
        bookingResponseData.Should().NotBeNull();
        var bookingId = bookingResponseData!.Id;
        
        bookingResponseData.Status.Should().Be(Contracts.Bookings.Enums.EBookingStatus.Pending);

        // 5. Autentica como Provider
        AuthenticateAsProvider(providerIdClaim);

        // 6. Provider confirma agendamento
        var confirmResponse = await ApiClient.PutAsync($"/api/v1/bookings/{bookingId}/confirm", null);
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 7. Busca agendamento pelo ID e checa se tá confirmado (Autenticado como cliente pra ver)
        AuthenticateAsUser(customerId.ToString());
        var getResponse = await ApiClient.GetAsync($"/api/v1/bookings/{bookingId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updatedBooking = await ReadJsonAsync<BookingDto>(getResponse);
        updatedBooking.Should().NotBeNull();
        updatedBooking!.Status.Should().Be(Contracts.Bookings.Enums.EBookingStatus.Confirmed);
    }

    private async Task<Guid> CreateTestProviderAsync()
    {
        AuthenticateAsAdmin();

        var userId = await CreateTestUserAsync();
        var name = $"ProviderX_{Guid.NewGuid():N}";
        var request = new
        {
            UserId = userId.ToString(),
            Name = name,
            Type = EProviderType.Individual,
            BusinessProfile = new
            {
                LegalName = name,
                FantasyName = name,
                Description = $"Test provider {name}",
                ContactInfo = new
                {
                    Email = $"{name}@example.com",
                    PhoneNumber = "+5511999999999"
                },
                PrimaryAddress = new
                {
                    Street = "Avenida Paulista",
                    Number = "1578",
                    Neighborhood = "Bela Vista",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01310-200",
                    Country = "Brasil"
                }
            }
        };

        var response = await ApiClient.PostAsJsonAsync("/api/v1/providers", request);
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location?.ToString();
        var providerId = ExtractIdFromLocation(location!);

        return providerId;
    }

    private void AuthenticateAsProvider(Guid providerId)
    {
        ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
        ConfigurableTestAuthenticationHandler.ConfigureProvider(providerId);
    }
}
