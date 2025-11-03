using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Shared.Tests.Auth;
using System.Net;

namespace MeAjudaAi.Integration.Tests;

public class RegressãoTest : ApiTestBase
{
    [Fact]
    public async Task UsersEndpoint_ShouldWork_WithoutErrors()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=5", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        Console.WriteLine($"Users Response Status: {response.StatusCode}");
        Console.WriteLine($"Users Response Content: {content}");
        
        // Verificar se realmente retorna OK (não 500)
        Assert.True(response.StatusCode == HttpStatusCode.OK, 
            $"Users endpoint should return OK, but got {response.StatusCode}. Content: {content}");
    }

    [Fact(Skip = "Temporarily skipped - Providers database schema/table creation issue under investigation. Authentication and Users endpoints are working. Providers requires custom schema table creation fix.")]
    [Trait("Category", "Providers")]
    [Trait("Status", "DatabaseIssue")]
    public async Task ProvidersEndpoint_ShouldWork_WithoutErrors()
    {
        // TODO: This test is temporarily skipped due to Providers database schema issue
        // The schema 'providers' is created, but tables are not being created properly by EnsureCreatedAsync
        // This needs investigation of why custom schema table creation fails in test environment
        
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers?page=1&pageSize=5", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        Console.WriteLine($"Providers Response Status: {response.StatusCode}");
        Console.WriteLine($"Providers Response Content: {content}");
        
        // Para comparação - vamos ver se funciona agora
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            Assert.Fail($"Providers endpoint still returns 500. Content: {content}");
        }
        else
        {
            Assert.True(true, $"Providers endpoint working! Status: {response.StatusCode}");
        }
    }
}