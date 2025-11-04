using System.Net;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Shared.Tests.Auth;

namespace MeAjudaAi.Integration.Tests;

public class RegressãoTest : ApiTestBase
{
    [Fact]
    public async Task UsersEndpoint_ShouldWork_WithoutErrors()
    {
        try
        {
            // Arrange
            ConfigurableTestAuthenticationHandler.ClearConfiguration();
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
        finally
        {
            ConfigurableTestAuthenticationHandler.ClearConfiguration();
        }
    }

    [Fact]
    [Trait("Category", "Providers")]
    [Trait("Status", "NeedsInvestigation")]
    public async Task ProvidersEndpoint_ShouldWork_WithoutErrors()
    {
        // NOTE: This test was previously skipped due to database schema issues,
        // but based on CI output, Providers endpoints are now returning 200 OK.
        // Re-enabling to verify current status.

        try
        {
            // Arrange
            ConfigurableTestAuthenticationHandler.ClearConfiguration();
            ConfigurableTestAuthenticationHandler.ConfigureAdmin();

            // Act
            var response = await Client.GetAsync("/api/v1/providers?page=1&pageSize=5", TestContext.Current.CancellationToken);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            // Assert
            Console.WriteLine($"Providers Response Status: {response.StatusCode}");
            Console.WriteLine($"Providers Response Content: {content}");

            // Providers endpoint should return OK (not 500)
            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Providers endpoint should return OK, but got {response.StatusCode}. Content: {content}");
        }
        finally
        {
            ConfigurableTestAuthenticationHandler.ClearConfiguration();
        }
    }
}
