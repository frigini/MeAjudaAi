using FluentAssertions;
using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace MeAjudaAi.ApiService.Tests.Extensions;

/// <summary>
/// Testes para SecurityExtensions - configuração de autenticação, autorização e CORS.
/// Objetivo: Aumentar coverage de 0% → 95%+
/// </summary>
[Trait("Category", "Unit")]
public class SecurityExtensionsTests
{
    private static IWebHostEnvironment CreateMockEnvironment(string environmentName = "Development")
    {
        var env = Substitute.For<IWebHostEnvironment>();
        env.EnvironmentName.Returns(environmentName);
        return env;
    }

    private static IConfiguration CreateConfiguration(Dictionary<string, string?> settings)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings!)
            .Build();
    }

    #region ValidateSecurityConfiguration Tests

    [Fact]
    public void ValidateSecurityConfiguration_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var environment = CreateMockEnvironment();

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(null!, environment);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateSecurityConfiguration_WithNullEnvironment_ShouldThrowArgumentNullException()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string?>());

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateSecurityConfiguration_InDevelopment_WithWildcardCors_ShouldNotThrow()
    {
        // Arrange - Development allows wildcards but still needs valid Keycloak config
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "*",
            ["Cors:AllowedMethods:0"] = "*",
            ["Cors:AllowedHeaders:0"] = "*",
            ["Cors:AllowCredentials"] = "false",
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "dev-realm",
            ["Keycloak:ClientId"] = "dev-client"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Development");

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);
        action.Should().NotThrow();
    }

    [Fact]
    public void ValidateSecurityConfiguration_InProduction_WithWildcardCors_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "*",
            ["Cors:AllowedMethods:0"] = "GET",
            ["Cors:AllowedHeaders:0"] = "Content-Type",
            ["Cors:AllowCredentials"] = "false",
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Production");

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Wildcard CORS origin*production*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_InProduction_WithHttpOrigins_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "http://localhost:3000",
            ["Cors:AllowedMethods:0"] = "GET",
            ["Cors:AllowedHeaders:0"] = "Content-Type",
            ["Cors:AllowCredentials"] = "false",
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Production");

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*HTTP origins*production*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_InProduction_WithManyOriginsAndCredentials_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://app1.com",
            ["Cors:AllowedOrigins:1"] = "https://app2.com",
            ["Cors:AllowedOrigins:2"] = "https://app3.com",
            ["Cors:AllowedOrigins:3"] = "https://app4.com",
            ["Cors:AllowedOrigins:4"] = "https://app5.com",
            ["Cors:AllowedOrigins:5"] = "https://app6.com",
            ["Cors:AllowedMethods:0"] = "GET",
            ["Cors:AllowedHeaders:0"] = "Content-Type",
            ["Cors:AllowCredentials"] = "true",
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Production");

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Too many allowed origins*credentials*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_InProduction_WithoutHttpsMetadata_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://app.com",
            ["Cors:AllowedMethods:0"] = "GET",
            ["Cors:AllowedHeaders:0"] = "Content-Type",
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client",
            ["Keycloak:RequireHttpsMetadata"] = "false"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Production");

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*RequireHttpsMetadata*true*production*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_InProduction_WithHttpKeycloakUrl_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://app.com",
            ["Cors:AllowedMethods:0"] = "GET",
            ["Cors:AllowedHeaders:0"] = "Content-Type",
            ["Keycloak:BaseUrl"] = "http://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Production");

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Keycloak BaseUrl*HTTPS*production*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_InProduction_WithHighClockSkew_ShouldThrowInvalidOperationException()
    {
        // Arrange - Complete production config with clock skew exceeding 30 minute limit
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://app.com",
            ["Cors:AllowedMethods:0"] = "GET",
            ["Cors:AllowedHeaders:0"] = "Content-Type",
            ["Cors:AllowCredentials"] = "false",
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client",
            ["Keycloak:RequireHttpsMetadata"] = "true",
            ["Keycloak:ClockSkew"] = "00:35:00", // ISSUE: > 30 minutes limit
            ["HttpsRedirection:Enabled"] = "true",
            ["AllowedHosts"] = "app.com"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Production");

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*ClockSkew*exceed 30 minutes*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_InProduction_WithValidHttpsRedirectionDisabled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://app.com",
            ["Cors:AllowedMethods:0"] = "GET",
            ["Cors:AllowedHeaders:0"] = "Content-Type",
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client",
            ["HttpsRedirection:Enabled"] = "false"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Production");

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*HTTPS redirection*enabled*production*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_InProduction_WithWildcardAllowedHosts_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://app.com",
            ["Cors:AllowedMethods:0"] = "GET",
            ["Cors:AllowedHeaders:0"] = "Content-Type",
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client",
            ["AllowedHosts"] = "*"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Production");

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*AllowedHosts*restricted*production*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_InTesting_ShouldSkipKeycloakValidation()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "*",
            ["Cors:AllowedMethods:0"] = "*",
            ["Cors:AllowedHeaders:0"] = "*",
            ["Cors:AllowCredentials"] = "false"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Testing");

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);
        action.Should().NotThrow();
    }

    [Fact]
    public void ValidateSecurityConfiguration_WithNegativeAnonymousLimits_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://app.com",
            ["Cors:AllowedMethods:0"] = "GET",
            ["Cors:AllowedHeaders:0"] = "Content-Type",
            ["AdvancedRateLimit:Anonymous:RequestsPerMinute"] = "-1",
            ["AdvancedRateLimit:Anonymous:RequestsPerHour"] = "100"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Development");

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Anonymous request limits*positive*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_InProduction_WithHighAnonymousLimits_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://app.com",
            ["Cors:AllowedMethods:0"] = "GET",
            ["Cors:AllowedHeaders:0"] = "Content-Type",
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client",
            ["AdvancedRateLimit:Anonymous:RequestsPerMinute"] = "200",
            ["AdvancedRateLimit:Anonymous:RequestsPerHour"] = "10000"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Production");

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Anonymous request limits*conservative*production*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_WithNegativeAuthenticatedLimits_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://app.com",
            ["Cors:AllowedMethods:0"] = "GET",
            ["Cors:AllowedHeaders:0"] = "Content-Type",
            ["AdvancedRateLimit:Authenticated:RequestsPerMinute"] = "100",
            ["AdvancedRateLimit:Authenticated:RequestsPerHour"] = "-500"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Development");

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Authenticated request limits*positive*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_WithValidProductionConfig_ShouldNotThrow()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://app.com",
            ["Cors:AllowedOrigins:1"] = "https://admin.app.com",
            ["Cors:AllowedMethods:0"] = "GET",
            ["Cors:AllowedMethods:1"] = "POST",
            ["Cors:AllowedHeaders:0"] = "Content-Type",
            ["Cors:AllowedHeaders:1"] = "Authorization",
            ["Cors:AllowCredentials"] = "true",
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client",
            ["Keycloak:RequireHttpsMetadata"] = "true",
            ["Keycloak:ClockSkew"] = "00:02:00",
            ["HttpsRedirection:Enabled"] = "true",
            ["AllowedHosts"] = "app.com;admin.app.com",
            ["AdvancedRateLimit:Anonymous:RequestsPerMinute"] = "50",
            ["AdvancedRateLimit:Anonymous:RequestsPerHour"] = "1000",
            ["AdvancedRateLimit:Authenticated:RequestsPerMinute"] = "200",
            ["AdvancedRateLimit:Authenticated:RequestsPerHour"] = "5000"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Production");

        // Act & Assert
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);
        action.Should().NotThrow();
    }

    #endregion

    #region AddCorsPolicy Tests

    [Fact]
    public void AddCorsPolicy_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string?>());
        var environment = CreateMockEnvironment();

        // Act & Assert
        var action = () => SecurityExtensions.AddCorsPolicy(null!, configuration, environment);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCorsPolicy_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var environment = CreateMockEnvironment();

        // Act & Assert
        var action = () => services.AddCorsPolicy(null!, environment);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCorsPolicy_WithNullEnvironment_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string?>());

        // Act & Assert
        var action = () => services.AddCorsPolicy(configuration, null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCorsPolicy_WithValidConfig_ShouldRegisterCorsOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Required for options validation
        services.AddSingleton<IConfiguration>(sp =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "https://app.com",
                ["Cors:AllowedMethods:0"] = "GET",
                ["Cors:AllowedHeaders:0"] = "Content-Type",
                ["Cors:AllowCredentials"] = "false"
            };
            return CreateConfiguration(settings);
        });
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var environment = CreateMockEnvironment();

        // Act
        services.AddCorsPolicy(configuration, environment);

        // Assert
        var provider = services.BuildServiceProvider();
        var corsOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<CorsOptions>>();
        corsOptions.Should().NotBeNull();
    }



    [Fact(Skip = "AddCorsPolicy doesn't validate production restrictions - validation happens in ValidateSecurityConfiguration which aggregates all security checks")]
    public void AddCorsPolicy_InProduction_WithWildcard_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Required for options validation
        var settings = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "*",
            ["Cors:AllowedMethods:0"] = "GET",
            ["Cors:AllowedHeaders:0"] = "Content-Type",
            ["Cors:AllowCredentials"] = "false"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Production");

        // Act & Assert
        var action = () => services.AddCorsPolicy(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Wildcard CORS origin*not allowed*production*");
    }

    #endregion

    #region AddEnvironmentAuthentication Tests

    [Fact]
    public void AddEnvironmentAuthentication_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string?>());
        var environment = CreateMockEnvironment();

        // Act & Assert
        var action = () => SecurityExtensions.AddEnvironmentAuthentication(null!, configuration, environment);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddEnvironmentAuthentication_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var environment = CreateMockEnvironment();

        // Act & Assert
        var action = () => services.AddEnvironmentAuthentication(null!, environment);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddEnvironmentAuthentication_WithNullEnvironment_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string?>());

        // Act & Assert
        var action = () => services.AddEnvironmentAuthentication(configuration, null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddEnvironmentAuthentication_InTesting_ShouldNotAddKeycloak()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string?>());
        var environment = CreateMockEnvironment("Testing");

        // Act
        services.AddEnvironmentAuthentication(configuration, environment);

        // Assert - Should not throw even without Keycloak config
        var action = () => services.BuildServiceProvider();
        action.Should().NotThrow();
    }

    #endregion

    #region AddKeycloakAuthentication Tests

    [Fact]
    public void AddKeycloakAuthentication_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string?>());
        var environment = CreateMockEnvironment();

        // Act & Assert
        var action = () => SecurityExtensions.AddKeycloakAuthentication(null!, configuration, environment);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddKeycloakAuthentication_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var environment = CreateMockEnvironment();

        // Act & Assert
        var action = () => services.AddKeycloakAuthentication(null!, environment);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddKeycloakAuthentication_WithNullEnvironment_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client"
        });

        // Act & Assert
        var action = () => services.AddKeycloakAuthentication(configuration, null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddKeycloakAuthentication_WithMissingBaseUrl_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var settings = new Dictionary<string, string?>
        {
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment();

        // Act & Assert
        var action = () => services.AddKeycloakAuthentication(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Keycloak BaseUrl*required*");
    }

    [Fact]
    public void AddKeycloakAuthentication_WithMissingRealm_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var settings = new Dictionary<string, string?>
        {
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:ClientId"] = "test-client"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment();

        // Act & Assert
        var action = () => services.AddKeycloakAuthentication(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Keycloak Realm*required*");
    }

    [Fact]
    public void AddKeycloakAuthentication_WithMissingClientId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var settings = new Dictionary<string, string?>
        {
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment();

        // Act & Assert
        var action = () => services.AddKeycloakAuthentication(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Keycloak ClientId*required*");
    }

    [Fact]
    public void AddKeycloakAuthentication_WithInvalidBaseUrl_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var settings = new Dictionary<string, string?>
        {
            ["Keycloak:BaseUrl"] = "not-a-valid-url",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment();

        // Act & Assert
        var action = () => services.AddKeycloakAuthentication(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*not a valid URL*");
    }

    [Fact]
    public void AddKeycloakAuthentication_WithExcessiveClockSkew_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var settings = new Dictionary<string, string?>
        {
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client",
            ["Keycloak:ClockSkew"] = "00:35:00" // 35 minutes in TimeSpan format - > 30 minute limit
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment();

        // Act & Assert
        var action = () => services.AddKeycloakAuthentication(configuration, environment);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*ClockSkew*exceed 30 minutes*");
    }

    [Fact]
    public void AddKeycloakAuthentication_WithValidConfig_ShouldRegisterAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(sp =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
                ["Keycloak:Realm"] = "test-realm",
                ["Keycloak:ClientId"] = "test-client",
                ["Keycloak:RequireHttpsMetadata"] = "true",
                ["Keycloak:ValidateIssuer"] = "true",
                ["Keycloak:ValidateAudience"] = "true",
                ["Keycloak:ClockSkew"] = "00:05:00"
            };
            return CreateConfiguration(settings);
        });
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var environment = CreateMockEnvironment();

        // Act
        services.AddKeycloakAuthentication(configuration, environment);

        // Assert
        var provider = services.BuildServiceProvider();
        var keycloakOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<KeycloakOptions>>();
        keycloakOptions.Should().NotBeNull();
        keycloakOptions!.Value.ClientId.Should().Be("test-client");
    }

    #endregion

    #region AddAuthorizationPolicies Tests

    [Fact]
    public void AddAuthorizationPolicies_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => SecurityExtensions.AddAuthorizationPolicies(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddAuthorizationPolicies_ShouldRegisterAuthorizationPolicies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();

        // Act
        services.AddAuthorizationPolicies();

        // Assert
        var provider = services.BuildServiceProvider();
        var authorizationOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Authorization.AuthorizationOptions>>();
        authorizationOptions.Should().NotBeNull();
        authorizationOptions!.Value.GetPolicy("SelfOrAdmin").Should().NotBeNull();
        authorizationOptions.Value.GetPolicy("AdminOnly").Should().NotBeNull();
        authorizationOptions.Value.GetPolicy("SuperAdminOnly").Should().NotBeNull();
    }

    #endregion

    #region KeycloakConfigurationLogger Tests

    [Fact]
    public async Task KeycloakConfigurationLogger_StartAsync_ShouldLogConfiguration()
    {
        // Arrange
        var keycloakOptions = Microsoft.Extensions.Options.Options.Create(new KeycloakOptions
        {
            BaseUrl = "https://keycloak.example.com",
            Realm = "test-realm",
            ClientId = "test-client"
        });

        var logger = Substitute.For<ILogger<KeycloakConfigurationLogger>>();
        var loggerInstance = new KeycloakConfigurationLogger(keycloakOptions, logger);

        // Act
        await loggerInstance.StartAsync(CancellationToken.None);

        // Assert
        logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Keycloak authentication configured")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task KeycloakConfigurationLogger_StopAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var keycloakOptions = Microsoft.Extensions.Options.Options.Create(new KeycloakOptions
        {
            BaseUrl = "https://keycloak.example.com",
            Realm = "test-realm",
            ClientId = "test-client"
        });

        var logger = Substitute.For<ILogger<KeycloakConfigurationLogger>>();
        var loggerInstance = new KeycloakConfigurationLogger(keycloakOptions, logger);

        // Act & Assert - Should complete without exceptions
        var act = () => loggerInstance.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    #endregion
}
