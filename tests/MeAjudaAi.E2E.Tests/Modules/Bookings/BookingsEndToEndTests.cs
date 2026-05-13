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
public class BookingsEndToEndTests : IClassFixture<TestContainerFixture>, IAsyncLifetime
{
    private readonly TestContainerFixture _fixture;

    public BookingsEndToEndTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        await _fixture.CleanupDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task CreateAndConfirmBooking_ShouldSucceed()
    {
        // Centraliza autenticação como admin no início do teste
        TestContainerFixture.AuthenticateAsAdmin();

        // 1. Criar um prestador feito com um providerId gerado
        var providerIdClaim = await CreateTestProviderAsync();

        // 1.5 Criar um serviço real
        var serviceId = await CreateTestServiceAsync();

        // 1.7 Vincular serviço ao prestador (Necessário devido à nova validação de segurança)
        // Adicionada retentativa simples para lidar com latência de infra
        var linked = false;
        for (int i = 0; i < 3; i++)
        {
            var response = await _fixture.ApiClient.PostAsync($"/api/v1/providers/{providerIdClaim}/services/{serviceId}", null);
            if (response.IsSuccessStatusCode)
            {
                linked = true;
                break;
            }
            await Task.Delay(1000);
        }
        linked.Should().BeTrue("service should be linked to provider before creating booking");
        
        // 2. Definir agenda para o prestador
        var tz = ResolveBrazilTimeZone();
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var localTomorrow = localNow.Date.AddDays(1);
        int dayOfWeek = (int)localTomorrow.DayOfWeek;
        
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

        var scheduleResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/bookings/schedule", scheduleRequest);
        scheduleResponse.EnsureSuccessStatusCode();

        // 3. Criar usuário (Cliente)
        var customerId = await _fixture.CreateTestUserAsync();
        TestContainerFixture.AuthenticateAsUser(customerId.ToString()); // Login como cliente

        // 4. Cliente cria um agendamento
        var localStart = new DateTime(localTomorrow.Year, localTomorrow.Month, localTomorrow.Day, 10, 0, 0);
        var localEnd = new DateTime(localTomorrow.Year, localTomorrow.Month, localTomorrow.Day, 11, 0, 0);
        
        var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
        var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, tz);

        var bookingRequest = new
        {
            providerId = providerIdClaim,
            serviceId = serviceId,
            start = utcStart.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            end = utcEnd.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        var createResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/bookings", bookingRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var bookingResponseData = await TestContainerFixture.ReadJsonAsync<BookingDto>(createResponse);
        bookingResponseData.Should().NotBeNull();
        var bookingId = bookingResponseData!.Id;
        bookingResponseData.Status.Should().Be(Contracts.Bookings.Enums.EBookingStatus.Pending);

        // 5. Autentica como Provider para confirmar
        AuthenticateAsProvider(providerIdClaim);

        // 6. Provider confirma agendamento
        var correlationId = Guid.NewGuid();
        var confirmRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/bookings/{bookingId}/confirm");
        confirmRequest.Headers.Add("X-Correlation-ID", correlationId.ToString());
        confirmRequest.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        
        var confirmResponse = await _fixture.ApiClient.SendAsync(confirmRequest);
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 7. Busca agendamento e checa status confirmado com Polling estendido (10 tentativas)
        TestContainerFixture.AuthenticateAsUser(customerId.ToString());
        
        var isConfirmed = false;
        BookingDto? updatedBooking = null;
        for (int i = 0; i < 10; i++)
        {
            var getResponse = await _fixture.ApiClient.GetAsync($"/api/v1/bookings/{bookingId}");
            if (getResponse.IsSuccessStatusCode)
            {
                updatedBooking = await TestContainerFixture.ReadJsonAsync<BookingDto>(getResponse);
                if (updatedBooking?.Status == Contracts.Bookings.Enums.EBookingStatus.Confirmed)
                {
                    isConfirmed = true;
                    break;
                }
            }
            await Task.Delay(1000); 
        }

        isConfirmed.Should().BeTrue($"booking {bookingId} should reach Confirmed status within polling period");
        updatedBooking!.Status.Should().Be(Contracts.Bookings.Enums.EBookingStatus.Confirmed);
    }

    private static TimeZoneInfo ResolveBrazilTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
        catch (Exception)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Could not resolve Brazil time zone on this system.", ex);
            }
        }
    }

    private async Task<Guid> CreateTestServiceAsync()
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                var categoryName = $"Category_{Guid.NewGuid():N}";
                var catResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = categoryName, displayOrder = 1 });
                catResponse.EnsureSuccessStatusCode();
                var catId = TestContainerFixture.ExtractIdFromLocation(catResponse.Headers.Location!.ToString());

                var serviceName = $"Service_{Guid.NewGuid():N}";
                var svcResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", new { name = serviceName, categoryId = catId });
                svcResponse.EnsureSuccessStatusCode();
                return TestContainerFixture.ExtractIdFromLocation(svcResponse.Headers.Location!.ToString());
            }
            catch (Exception ex)
            {
                if (i == 2) throw new Exception("Failed to create test service after retries", ex);
                await Task.Delay(1000);
            }
        }
        throw new Exception("Failed to create test service after retries");
    }

    private async Task<Guid> CreateTestProviderAsync()
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                var userId = await _fixture.CreateTestUserAsync();
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
                        ContactInfo = new { Email = $"{name}@example.com", PhoneNumber = "+5511999999999" },
                        PrimaryAddress = new { Street = "Av Paulista", Number = "1578", Neighborhood = "Bela Vista", City = "São Paulo", State = "SP", ZipCode = "01310-200", Country = "Brasil" }
                    }
                };

                var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/providers", request);
                response.EnsureSuccessStatusCode();
                return TestContainerFixture.ExtractIdFromLocation(response.Headers.Location!.ToString());
            }
            catch (Exception ex)
            {
                if (i == 2) throw new Exception("Failed to create test provider after retries", ex);
                await Task.Delay(1000);
            }
        }
        throw new Exception("Failed to create test provider after retries");
    }

    private static void AuthenticateAsProvider(Guid providerId)
    {
        ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
        ConfigurableTestAuthenticationHandler.ConfigureProvider(providerId, Guid.NewGuid().ToString());
    }
}
