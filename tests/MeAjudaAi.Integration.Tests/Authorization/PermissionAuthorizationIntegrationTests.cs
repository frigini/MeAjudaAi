using System.Security.Claims;
using System.Text.Encodings.Web;
using MeAjudaAi.Shared.Authorization;
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
public class PermissionAuthorizationIntegrationTests : IClassFixture<PermissionAuthorizationIntegrationTests.TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PermissionAuthorizationIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task EndpointWithPermissionRequirement_WithValidPermission_ShouldReturnSuccess()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("sub", "test-user-123"),
            new Claim(CustomClaimTypes.Permission, Permission.UsersRead.GetValue())
        };

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddTestAuthentication(claims);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/test/users-read");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EndpointWithPermissionRequirement_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("sub", "test-user-123"),
            new Claim(CustomClaimTypes.Permission, Permission.UsersProfile.GetValue()) // Permissão errada
        };

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddTestAuthentication(claims);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/test/users-read");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EndpointWithMultiplePermissions_WithAllPermissions_ShouldReturnSuccess()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("sub", "test-user-123"),
            new Claim(CustomClaimTypes.Permission, Permission.UsersDelete.GetValue()),
            new Claim(CustomClaimTypes.Permission, Permission.AdminUsers.GetValue())
        };

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddTestAuthentication(claims);
            });
        }).CreateClient();

        // Act
        var response = await client.DeleteAsync("/test/users-delete");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EndpointWithMultiplePermissions_WithPartialPermissions_ShouldReturnForbidden()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("sub", "test-user-123"),
            new Claim(CustomClaimTypes.Permission, Permission.UsersDelete.GetValue())
            // Faltando permissão AdminUsers
        };

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddTestAuthentication(claims);
            });
        }).CreateClient();

        // Act
        var response = await client.DeleteAsync("/test/users-delete");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EndpointWithAnyPermission_WithOneOfRequiredPermissions_ShouldReturnSuccess()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("sub", "test-user-123"),
            new Claim(CustomClaimTypes.Permission, Permission.AdminUsers.GetValue())
            // Tem AdminUsers mas não UsersRead - ainda deve funcionar para requisito "any"
        };

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddTestAuthentication(claims);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/test/users-read-or-admin");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EndpointWithSystemAdminRequirement_WithSystemAdminClaim_ShouldReturnSuccess()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("sub", "test-user-123"),
            new Claim(CustomClaimTypes.IsSystemAdmin, "true")
        };

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddTestAuthentication(claims);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/test/system-admin");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EndpointWithModulePermission_WithValidModulePermissions_ShouldReturnSuccess()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("sub", "test-user-123"),
            new Claim(CustomClaimTypes.Permission, Permission.AdminUsers.GetValue()),
            new Claim(CustomClaimTypes.Permission, Permission.UsersList.GetValue())
        };

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddTestAuthentication(claims);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/test/users-module-admin");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnauthenticatedRequest_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/test/users-read");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
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

                // Adiciona autorização baseada em permissões
                services.AddPermissionBasedAuthorization();
                services.AddUsersModule(configuration);

                // Adiciona autenticação de teste
                services.AddAuthentication("Test")
                    .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });

                services.AddAuthorization();
            });

            builder.Configure(app =>
            {
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseRouting();

                app.UseEndpoints(endpoints =>
                {
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
                        .RequireAuthorization();
                });
            });
        }
    }
}

/// <summary>
/// Extensões para configurar autenticação de teste.
/// </summary>
public static class TestAuthenticationExtensions
{
    public static IServiceCollection AddTestAuthentication(this IServiceCollection services, Claim[] claims)
    {
        services.Configure<TestAuthenticationSchemeOptions>(options =>
        {
            options.Claims = claims;
        });

        return services;
    }
}

/// <summary>
/// Opções para o esquema de autenticação de teste.
/// </summary>
public class TestAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public Claim[] Claims { get; set; } = Array.Empty<Claim>();
}

/// <summary>
/// Handler de autenticação para testes.
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<TestAuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Options.Claims?.Any() == true)
        {
            var identity = new ClaimsIdentity(Options.Claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.NoResult());
    }
}
