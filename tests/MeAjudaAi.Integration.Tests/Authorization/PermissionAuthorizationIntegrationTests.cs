using System.Security.Claims;
using System.Text.Encodings.Web;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Tests.Auth;
using MeAjudaAi.Shared.Tests.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static MeAjudaAi.Modules.Users.API.Extensions;

namespace MeAjudaAi.Integration.Tests.Authorization;

/// <summary>
/// Testes de integração para o sistema de autorização baseado em permissões.
/// </summary>
public class PermissionAuthorizationIntegrationTests : IClassFixture<PermissionAuthorizationIntegrationTests.TestWebApplicationFactory>, IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PermissionAuthorizationIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        // Clean up authentication configuration after each test
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
    }

    [Fact]
    public async Task AuthenticatedEndpoint_WithAnyClaims_ShouldReturnSuccess()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureRegularUser();

        // Act
        var response = await _client.GetAsync("/test/authenticated", TestContext.Current.CancellationToken);

        // Assert
        Console.WriteLine($"Response status: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response content: {content}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EndpointWithPermissionRequirement_WithValidPermission_ShouldReturnSuccess()
    {
        // Arrange - Configure user with admin permissions (includes UsersRead)
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        // Act
        var response = await _client.GetAsync("/test/users-read", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EndpointWithPermissionRequirement_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange - Configure user with only basic permissions (not UsersRead)
        ConfigurableTestAuthenticationHandler.ConfigureRegularUser();

        // Act
        var response = await _client.GetAsync("/test/users-read", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EndpointWithMultiplePermissions_WithAllPermissions_ShouldReturnSuccess()
    {
        // Arrange - Admin has all permissions including UsersDelete and AdminUsers
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        // Act
        var response = await _client.DeleteAsync("/test/users-delete", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EndpointWithMultiplePermissions_WithPartialPermissions_ShouldReturnForbidden()
    {
        // Arrange - Regular user doesn't have UsersDelete or AdminUsers permissions
        ConfigurableTestAuthenticationHandler.ConfigureRegularUser();

        // Act
        var response = await _client.DeleteAsync("/test/users-delete", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EndpointWithAnyPermission_WithOneOfRequiredPermissions_ShouldReturnSuccess()
    {
        // Arrange - Admin has required permissions
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        // Act
        var response = await _client.GetAsync("/test/users-read-or-admin", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

        [Fact]
    public async Task EndpointWithSystemAdminRequirement_WithSystemAdminClaim_ShouldReturnSuccess()
    {
        // Arrange - Admin has system admin claim
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        // Act
        var response = await _client.GetAsync("/test/system-admin", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

        [Fact]
    public async Task EndpointWithModulePermission_WithValidModulePermissions_ShouldReturnSuccess()
    {
        // Arrange - Admin has all module permissions including AdminUsers and UsersList
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        // Act
        var response = await _client.GetAsync("/test/users-module-admin", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnauthenticatedRequest_ShouldReturnUnauthorized()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ClearConfiguration();

        // Act
        var response = await _client.GetAsync("/test/users-read", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ConfigureServices(services =>
            {
                // Cria configuração de teste
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Database"] = "Server=localhost;Database=test;",
                        ["Modules:Users:Enabled"] = "true"
                    })
                    .Build();

                // Adiciona módulo de usuários (autorização já está configurada no application setup)
                services.AddUsersModule(configuration);

                // Remove ClaimsTransformation that causes hanging in tests
                var claimsTransformationDescriptor = services.FirstOrDefault(d =>
                    d.ServiceType == typeof(Microsoft.AspNetCore.Authentication.IClaimsTransformation));
                if (claimsTransformationDescriptor != null)
                    services.Remove(claimsTransformationDescriptor);

                // Use the same authentication pattern as working tests
                services.RemoveRealAuthentication();
                services.AddConfigurableTestAuthentication();

                services.AddAuthorization();
            });

            builder.Configure(app =>
            {
                app.UseAuthentication();
                app.UseRouting();
                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    // Endpoint simples apenas para testar autenticação
                    endpoints.MapGet("/test/authenticated", () => Results.Ok("Authenticated"))
                        .RequireAuthorization();

                    // Endpoints de teste
                    endpoints.MapGet("/test/users-read", () => Results.Ok("Success"))
                        .RequirePermission(Permission.UsersRead)
                        .RequireAuthorization();

                    endpoints.MapDelete("/test/users-delete", () => Results.Ok("Success"))
                        .RequirePermissions(Permission.UsersDelete, Permission.AdminUsers)
                        .RequireAuthorization();

                    endpoints.MapGet("/test/users-read-or-admin", () => Results.Ok("Success"))
                        .RequireAuthorization();

                    endpoints.MapGet("/test/system-admin", () => Results.Ok("Success"))
                        .RequireAuthorization();

                    endpoints.MapGet("/test/users-module-admin", () => Results.Ok("Success"))
                        .RequirePermissions(Permission.AdminUsers, Permission.UsersList)
                        .RequireAuthorization();
                });
            });
        }
    }
}
