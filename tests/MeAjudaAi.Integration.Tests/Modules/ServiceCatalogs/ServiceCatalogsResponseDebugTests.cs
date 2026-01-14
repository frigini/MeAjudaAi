using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Shared.Serialization;

namespace MeAjudaAi.Integration.Tests.Modules.ServiceCatalogs;

public class ServiceCatalogsResponseDebugTests(ITestOutputHelper testOutput) : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.ServiceCatalogs;

    [Fact(Skip = "Diagnostic test - enable only when debugging response format issues")]
    [Trait("Category", "Debug")]
    public async Task Debug_CreateServiceCategory_ResponseFormat()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var categoryData = new
        {
            name = $"Debug Category {Guid.NewGuid():N}",
            description = "Debug test",
            displayOrder = 1
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryData);

        // Assert - Log everything
        var content = await response.Content.ReadAsStringAsync();
        testOutput.WriteLine($"Status Code: {response.StatusCode}");
        testOutput.WriteLine($"Content Type: {response.Content.Headers.ContentType}");
        testOutput.WriteLine($"Raw Response: {content}");
        testOutput.WriteLine($"Response Length: {content.Length}");

        JsonElement json;
        try
        {
            // Use shared JSON deserialization for consistency with API serialization options
            json = JsonSerializer.Deserialize<JsonElement>(content, SerializationDefaults.Api);
            testOutput.WriteLine($"JSON ValueKind: {json.ValueKind}");

            if (json.ValueKind == JsonValueKind.Object)
            {
                testOutput.WriteLine("Properties:");
                foreach (var prop in json.EnumerateObject())
                {
                    testOutput.WriteLine($"  {prop.Name}: {prop.Value.ValueKind} = {prop.Value}");
                }
            }
        }
        catch (Exception ex)
        {
            testOutput.WriteLine($"JSON Parsing Error: {ex.Message}");
            throw; // Re-throw to fail the test
        }

        // Validate expected DTO shape outside try/catch so assertions are not swallowed
        if (json.ValueKind == JsonValueKind.Object)
        {
            json.TryGetProperty("id", out _).Should().BeTrue("DTO should have 'id' property");
            json.TryGetProperty("name", out _).Should().BeTrue("DTO should have 'name' property");
        }

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
