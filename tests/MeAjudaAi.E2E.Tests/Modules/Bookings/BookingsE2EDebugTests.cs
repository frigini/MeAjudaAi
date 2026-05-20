using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Modules.Bookings;

public class BookingsE2EDebugTests : BaseTestContainerTest
{
    [Fact]
    public async Task Debug_CreateBooking_ShouldLogDetailsOnFailure()
    {
        AuthenticateAsAdmin();
        var client = ApiClient;
        
        // 1. Create a category to deactivate
        var categoryRequest = new { Name = $"Cat_{Guid.NewGuid():N}", Description = "Desc", DisplayOrder = 1 };
        var catResponse = await client.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryRequest);
        if (catResponse.Headers.Location == null)
        {
            var content = await catResponse.Content.ReadAsStringAsync();
            throw new Exception($"DEBUG: Category creation failed. Status: {catResponse.StatusCode}. Content: {content}");
        }
        var catId = TestContainerFixture.ExtractIdFromLocation(catResponse.Headers.Location.ToString());
        
        var svcResponse = await client.PostAsJsonAsync("/api/v1/service-catalogs/services", new { name = $"Svc_{Guid.NewGuid():N}", categoryId = catId });
        if (svcResponse.Headers.Location == null)
        {
            var content = await svcResponse.Content.ReadAsStringAsync();
            throw new Exception($"DEBUG: Service creation failed. Status: {svcResponse.StatusCode}. Content: {content}");
        }
        var serviceId = TestContainerFixture.ExtractIdFromLocation(svcResponse.Headers.Location.ToString());

        // 2. Criar uma reserva
        var startTime = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endTime = startTime.AddHours(1); // 1 hora de duração
        var bookingRequest = new { ServiceId = serviceId, ClientId = Guid.NewGuid(), StartTime = startTime, EndTime = endTime };
        
        var response = await client.PostAsJsonAsync("/api/v1/bookings", bookingRequest);
        
        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"DEBUG: Booking creation failed with {response.StatusCode}. Content: {content}");
        }
        
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
    }
}
