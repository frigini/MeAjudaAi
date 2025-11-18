using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

namespace MeAjudaAi.Integration.Tests.Modules.Catalogs;

public class CatalogsResponseDebugTest(ITestOutputHelper testOutput) : ApiTestBase
{
    [Fact]
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
        var response = await Client.PostAsJsonAsync("/api/v1/catalogs/categories", categoryData);

        // Assert - Log everything
        var content = await response.Content.ReadAsStringAsync();
        testOutput.WriteLine($"Status Code: {response.StatusCode}");
        testOutput.WriteLine($"Content Type: {response.Content.Headers.ContentType}");
        testOutput.WriteLine($"Raw Response: {content}");
        testOutput.WriteLine($"Response Length: {content.Length}");

        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(content);
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
        }

        // Don't fail, just log
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
