using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.ServiceCatalogs;

public class ServiceCatalogsDebugTests : BaseApiTest
{
    [Fact]
    public async Task Debug_CreateServiceCategory_ShouldLogDetailsOnFailure()
    {
        AuthConfig.ConfigureAdmin();
        
        var categoryRequest = new { Name = "DebugCategory", Description = "Desc", DisplayOrder = 1 };
        
        var response = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryRequest);
        
        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            var problem = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
            var detail = problem?.ContainsKey("detail") == true ? problem["detail"].ToString() : "No detail provided";
            throw new Exception($"DEBUG: Request failed with {response.StatusCode}. Detail: {detail}. Full Content: {content}");
        }
        
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
    }
}
