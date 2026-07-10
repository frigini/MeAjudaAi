using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.E2E.Tests.Base;
using System.Net.Http.Json;

namespace MeAjudaAi.E2E.Tests.Modules.Bookings;

[Trait("Category", "E2E")]
[Trait("Module", "Bookings")]
public class BookingsEndToEndTests(EventsEnabledTestContainerFixture fixture, ITestOutputHelper output) : BaseEventsE2ETest(fixture)
{

    [Fact]
    public async Task CreateAndConfirmBooking_ShouldSucceed()
    {
        // Centraliza autenticação como admin no início do teste
        EventsEnabledTestContainerFixture.AuthenticateAsAdmin();

        var baseUtcNow = DateTime.UtcNow;

        // 1. Criar um prestador feito com um providerId gerado
        var providerIdClaim = await Fixture.CreateTestProviderAsync(await Fixture.CreateTestUserAsync());

        // 1.5 Criar um serviço real
        var serviceId = await Fixture.CreateTestServiceViaApiAsync();

        // 1.7 Vincular serviço ao prestador (Necessário devido à nova validação de segurança)
        await Fixture.LinkServiceToProviderAsync(providerIdClaim, serviceId);
        
        // 2. Definir agenda para o prestador
        // Usar lógica de timezone para derivar datas
        var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(baseUtcNow, tz);
        var localTomorrow = localNow.Date.AddDays(1);
        int dayOfWeek = (int)localTomorrow.DayOfWeek;

        var start1Time = localTomorrow.AddHours(10);
        var end1Time = localTomorrow.AddHours(11);
        var start2Time = localTomorrow.AddHours(14);
        var end2Time = localTomorrow.AddHours(15);
        
        var start1 = new DateTimeOffset(start1Time, tz.GetUtcOffset(start1Time));
        var end1 = new DateTimeOffset(end1Time, tz.GetUtcOffset(end1Time));
        var start2 = new DateTimeOffset(start2Time, tz.GetUtcOffset(start2Time));
        var end2 = new DateTimeOffset(end2Time, tz.GetUtcOffset(end2Time));
        
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
                        new { start = start1.ToString("O"), end = end1.ToString("O") },
                        new { start = start2.ToString("O"), end = end2.ToString("O") }
                    }
                }
            }
        };

        // Envia como admin ou provider (Admin pode setar p/ qq um pelo request body, Provider baseia no claim)
        var scheduleResponse = await Fixture.ApiClient.PostAsJsonAsync("/api/v1/bookings/schedule", scheduleRequest);
        if (!scheduleResponse.IsSuccessStatusCode)
        {
            var content = await scheduleResponse.Content.ReadAsStringAsync();
            output.WriteLine($"Schedule POST failed: {scheduleResponse.StatusCode} - {content}");
        }
        scheduleResponse.EnsureSuccessStatusCode();

        // 3. Criar usuário (Cliente)
        var customerId = await Fixture.CreateTestUserAsync();
        TestContainerFixture.AuthenticateAsUser(customerId.ToString()); // Login como cliente

        // 4. Cliente cria um agendamento
        // Usando horários que caiam dentro do slot de 10h-11h local do prestador (Brasília UTC-3)
        // Converter horários locais para UTC
        var localStart = new DateTime(localTomorrow.Year, localTomorrow.Month, localTomorrow.Day, 10, 0, 0);
        var localEnd = new DateTime(localTomorrow.Year, localTomorrow.Month, localTomorrow.Day, 11, 0, 0);
        
        var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
        var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, tz);

        var startIso = utcStart.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var endIso = utcEnd.ToString("yyyy-MM-ddTHH:mm:ssZ");

        var bookingRequest = new
        {
            providerId = providerIdClaim,
            serviceId = serviceId,
            start = startIso,
            end = endIso
        };

        var createResponse = await Fixture.ApiClient.PostAsJsonAsync("/api/v1/bookings", bookingRequest);
        
        // Se retornar BadRequest, quer dizer que tem algum erro de fuso e validação de availability, 
        // mas para fins de teste garantimos 201 ou tratamos.
        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            var contentMsg = await createResponse.Content.ReadAsStringAsync();
            output.WriteLine($"Creation failed: {contentMsg}");
        }
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var bookingResponseData = await TestContainerFixture.ReadJsonAsync<ModuleBookingDto>(createResponse);
        bookingResponseData.Should().NotBeNull();
        var bookingId = bookingResponseData!.Id;
        
        bookingResponseData.Status.Should().Be(MeAjudaAi.Contracts.Modules.Bookings.Enums.EBookingStatus.Pending);

        // 5. Autentica como Provider
        TestContainerFixture.AuthenticateAsProvider(providerIdClaim);

        // 6. Provider confirma agendamento
        var confirmResponse = await Fixture.ApiClient.PutAsync($"/api/v1/bookings/{bookingId}/confirm", new System.Net.Http.StringContent("", System.Text.Encoding.UTF8, "application/json"));
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 7. Busca agendamento pelo ID e checa se tá confirmado (Autenticado como cliente pra ver)
        TestContainerFixture.AuthenticateAsUser(customerId.ToString());
        var getResponse = await Fixture.ApiClient.GetAsync($"/api/v1/bookings/{bookingId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updatedBooking = await TestContainerFixture.ReadJsonAsync<ModuleBookingDto>(getResponse);
        updatedBooking.Should().NotBeNull();
        updatedBooking!.Status.Should().Be(MeAjudaAi.Contracts.Modules.Bookings.Enums.EBookingStatus.Confirmed);
    }
}
