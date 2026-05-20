using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Modules.ServiceCatalogs;

public class ServiceCatalogsE2EDebugTests : BaseTestContainerTest
{
    [Fact]
    public async Task Debug_DeactivateCategory_ShouldLogDetailsOnFailure()
    {
        // Authenticate the test correctly
        TestContainerFixture.AuthenticateAsAdmin();
        
        // Use the ApiClient property from BaseTestContainerTest
        var client = ApiClient;
        
        // 1. Create a category to deactivate
        var categoryRequest = new { Name = "DebugDeactivate", Description = "Desc", DisplayOrder = 1 };
        var createResponse = await client.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryRequest);
        
        if (createResponse.Headers.Location == null)
        {
            var content = await createResponse.Content.ReadAsStringAsync();
            throw new Exception($"DEBUG: Create failed. Status: {createResponse.StatusCode}. Content: {content}");
        }
        
        var categoryId = TestContainerFixture.ExtractIdFromLocation(createResponse.Headers.Location!.ToString());

        // 2. Attempt deactivation
        var response = await client.PostAsync($"/api/v1/service-catalogs/categories/{categoryId}/deactivate", null);
        
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"DEBUG: Deactivate failed with {response.StatusCode}. Full Content: {content}");
        }
        
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
