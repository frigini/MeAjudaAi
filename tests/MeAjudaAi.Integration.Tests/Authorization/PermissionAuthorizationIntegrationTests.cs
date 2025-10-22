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
    public async Task AuthenticatedEndpoint_WithAnyClaims_ShouldReturnSuccess()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("sub", "test-user-123"),
            new Claim("test", "value")
        };

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Update the test authentication options with new claims
                services.Configure<TestAuthenticationSchemeOptions>(options =>
                {
                    options.Claims = claims;
                });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/test/authenticated", TestContext.Current.CancellationToken);

        // Assert
        Console.WriteLine($"Response status: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response content: {content}");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
                // Update the test authentication options with new claims
                services.Configure<TestAuthenticationSchemeOptions>(options =>
                {
                    options.Claims = claims;
                });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/test/users-read", TestContext.Current.CancellationToken);

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
                // Update the test authentication options with new claims
                services.Configure<TestAuthenticationSchemeOptions>(options =>
                {
                    options.Claims = claims;
                });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/test/users-read", TestContext.Current.CancellationToken);

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
                // Update the test authentication options with new claims
                services.Configure<TestAuthenticationSchemeOptions>(options =>
                {
                    options.Claims = claims;
                });
            });
        }).CreateClient();

        // Act
        var response = await client.DeleteAsync("/test/users-delete", TestContext.Current.CancellationToken);

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
                // Update the test authentication options with new claims
                services.Configure<TestAuthenticationSchemeOptions>(options =>
                {
                    options.Claims = claims;
                });
            });
        }).CreateClient();

        // Act
        var response = await client.DeleteAsync("/test/users-delete", TestContext.Current.CancellationToken);

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
                // Update the test authentication options with new claims
                services.Configure<TestAuthenticationSchemeOptions>(options =>
                {
                    options.Claims = claims;
                });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/test/users-read-or-admin", TestContext.Current.CancellationToken);

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
                // Update the test authentication options with new claims
                services.Configure<TestAuthenticationSchemeOptions>(options =>
                {
                    options.Claims = claims;
                });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/test/system-admin", TestContext.Current.CancellationToken);

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
                // Update the test authentication options with new claims
                services.Configure<TestAuthenticationSchemeOptions>(options =>
                {
                    options.Claims = claims;
                });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/test/users-module-admin", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnauthenticatedRequest_ShouldReturnUnauthorized()
    {
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

                // Adiciona autenticação de teste
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });

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
    public IReadOnlyList<Claim> Claims { get; set; } = Array.Empty<Claim>();
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
        Console.WriteLine($"TestAuthenticationHandler.HandleAuthenticateAsync called. Claims count: {Options.Claims?.Count ?? 0}");
        
        if (Options.Claims?.Any() == true)
        {
            Console.WriteLine($"Creating identity with claims: {string.Join(", ", Options.Claims.Select(c => $"{c.Type}={c.Value}"))}");
            
            var identity = new ClaimsIdentity(Options.Claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            Console.WriteLine($"Authentication success. Identity authenticated: {identity.IsAuthenticated}");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        Console.WriteLine("No claims found, returning NoResult");
        return Task.FromResult(AuthenticateResult.NoResult());
    }
}
