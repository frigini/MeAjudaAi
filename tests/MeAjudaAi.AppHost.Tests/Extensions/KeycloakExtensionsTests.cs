using Aspire.Hosting;
using FluentAssertions;
using MeAjudaAi.AppHost.Extensions;
using Xunit;

namespace MeAjudaAi.AppHost.Tests.Extensions;

/// <summary>
/// Testes unitários para KeycloakExtensions (Aspire/Keycloak setup).
/// Valida configuração de desenvolvimento, produção e teste.
/// </summary>
public sealed class KeycloakExtensionsTests : IDisposable
{
    private readonly IDistributedApplicationBuilder _builder;

    public KeycloakExtensionsTests()
    {
        var options = new DistributedApplicationOptions
        {
            Args = Array.Empty<string>(),
            DisableDashboard = true
        };
        _builder = DistributedApplication.CreateBuilder(options);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    #region MeAjudaAiKeycloakOptions Tests

    [Fact]
    public void MeAjudaAiKeycloakOptions_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var options = new MeAjudaAiKeycloakOptions();

        // Assert
        options.AdminUsername.Should().Be("admin");
        options.AdminPassword.Should().Be("admin123");
        options.DatabaseHost.Should().Be("postgres-local");
        options.DatabasePort.Should().Be("5432");
        options.DatabaseName.Should().Be("meajudaai");
        options.DatabaseSchema.Should().Be("identity");
        options.DatabaseUsername.Should().Be("postgres");
        options.ExposeHttpEndpoint.Should().BeTrue();
        options.ImportRealm.Should().Be("/opt/keycloak/data/import/meajudaai-realm.json");
        options.IsTestEnvironment.Should().BeFalse();
    }

    [Fact]
    public void MeAjudaAiKeycloakOptions_DatabasePassword_ShouldReadFromEnvironmentVariable()
    {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "test-password");

        try
        {
            // Act
            var options = new MeAjudaAiKeycloakOptions();

            // Assert
            options.DatabasePassword.Should().Be("test-password");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", originalValue);
        }
    }

    [Fact]
    public void MeAjudaAiKeycloakOptions_DatabasePassword_ShouldFallbackToDefault()
    {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", null);

        try
        {
            // Act
            var options = new MeAjudaAiKeycloakOptions();

            // Assert
            options.DatabasePassword.Should().Be("dev123");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", originalValue);
        }
    }

    [Fact]
    public void MeAjudaAiKeycloakOptions_Hostname_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var options = new MeAjudaAiKeycloakOptions();

        // Assert
        options.Hostname.Should().BeNull();
    }

    [Fact]
    public void MeAjudaAiKeycloakOptions_CustomValues_ShouldBeSettable()
    {
        // Arrange & Act
        var options = new MeAjudaAiKeycloakOptions
        {
            AdminUsername = "custom-admin",
            AdminPassword = "custom-password",
            DatabaseHost = "custom-host",
            DatabasePort = "5433",
            DatabaseName = "custom-db",
            DatabaseSchema = "custom_schema",
            DatabaseUsername = "custom-user",
            DatabasePassword = "custom-pass",
            Hostname = "keycloak.example.com",
            ExposeHttpEndpoint = false,
            ImportRealm = "/custom/realm.json",
            IsTestEnvironment = true
        };

        // Assert
        options.AdminUsername.Should().Be("custom-admin");
        options.AdminPassword.Should().Be("custom-password");
        options.DatabaseHost.Should().Be("custom-host");
        options.DatabasePort.Should().Be("5433");
        options.DatabaseName.Should().Be("custom-db");
        options.DatabaseSchema.Should().Be("custom_schema");
        options.DatabaseUsername.Should().Be("custom-user");
        options.DatabasePassword.Should().Be("custom-pass");
        options.Hostname.Should().Be("keycloak.example.com");
        options.ExposeHttpEndpoint.Should().BeFalse();
        options.ImportRealm.Should().Be("/custom/realm.json");
        options.IsTestEnvironment.Should().BeTrue();
    }

    #endregion

    #region AddMeAjudaAiKeycloak (Development) Tests

    [Fact]
    public void AddMeAjudaAiKeycloak_WithDefaultOptions_ShouldReturnResult()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloak();

        // Assert
        result.Should().NotBeNull();
        result.Keycloak.Should().NotBeNull();
        result.AuthUrl.Should().Be("http://localhost:8080");
        result.AdminUrl.Should().Be("http://localhost:8080/admin");
    }

    [Fact]
    public void AddMeAjudaAiKeycloak_WithDefaultOptions_ShouldConfigureKeycloakResource()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloak();

        // Assert
        result.Keycloak.Resource.Name.Should().Be("keycloak");
    }

    [Fact]
    public void AddMeAjudaAiKeycloak_WithCustomOptions_ShouldApplyConfiguration()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloak(opts =>
        {
            opts.AdminUsername = "test-admin";
            opts.AdminPassword = "test-pass";
            opts.DatabaseSchema = "test_schema";
            opts.ExposeHttpEndpoint = true;
        });

        // Assert
        result.Should().NotBeNull();
        result.Keycloak.Should().NotBeNull();
    }

    [Fact]
    public void AddMeAjudaAiKeycloak_WithNullImportRealm_ShouldNotConfigureImport()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloak(opts =>
        {
            opts.ImportRealm = null;
        });

        // Assert
        result.Should().NotBeNull();
        result.Keycloak.Should().NotBeNull();
    }

    [Fact]
    public void AddMeAjudaAiKeycloak_WithEmptyImportRealm_ShouldNotConfigureImport()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloak(opts =>
        {
            opts.ImportRealm = string.Empty;
        });

        // Assert
        result.Should().NotBeNull();
        result.Keycloak.Should().NotBeNull();
    }

    [Fact]
    public void AddMeAjudaAiKeycloak_WithExposeHttpEndpointFalse_ShouldNotExposeEndpoint()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloak(opts =>
        {
            opts.ExposeHttpEndpoint = false;
        });

        // Assert
        result.Should().NotBeNull();
        result.Keycloak.Should().NotBeNull();
    }

    [Fact]
    public void AddMeAjudaAiKeycloak_MultipleCustomizations_ShouldApplyAll()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloak(opts =>
        {
            opts.AdminUsername = "custom";
            opts.DatabaseSchema = "custom_identity";
            opts.DatabaseHost = "custom-postgres";
            opts.DatabasePort = "5433";
        });

        // Assert
        result.Should().NotBeNull();
        result.AuthUrl.Should().Be("http://localhost:8080");
    }

    #endregion

    #region AddMeAjudaAiKeycloakProduction Tests

    [Fact]
    public void AddMeAjudaAiKeycloakProduction_WithoutRequiredEnvVars_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var originalKeycloakPassword = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD");
        var originalPostgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", null);
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", null);

        try
        {
            // Act & Assert
            var act = () => _builder.AddMeAjudaAiKeycloakProduction();
            
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*KEYCLOAK_ADMIN_PASSWORD*required*");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", originalKeycloakPassword);
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", originalPostgresPassword);
        }
    }

    [Fact]
    public void AddMeAjudaAiKeycloakProduction_WithoutPostgresPassword_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var originalKeycloakPassword = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD");
        var originalPostgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", "prod-admin-pass");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", null);

        try
        {
            // Act & Assert
            var act = () => _builder.AddMeAjudaAiKeycloakProduction();
            
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*POSTGRES_PASSWORD*required*");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", originalKeycloakPassword);
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", originalPostgresPassword);
        }
    }

    [Fact]
    public void AddMeAjudaAiKeycloakProduction_WithRequiredEnvVars_ShouldReturnResult()
    {
        // Arrange
        var originalKeycloakPassword = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD");
        var originalPostgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        var originalHostname = Environment.GetEnvironmentVariable("KEYCLOAK_HOSTNAME");
        
        Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", "prod-admin-pass");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "prod-db-pass");
        Environment.SetEnvironmentVariable("KEYCLOAK_HOSTNAME", "keycloak.example.com");

        try
        {
            // Act
            var result = _builder.AddMeAjudaAiKeycloakProduction(opts =>
            {
                opts.Hostname = "keycloak.example.com";
            });

            // Assert
            result.Should().NotBeNull();
            result.Keycloak.Should().NotBeNull();
            result.AuthUrl.Should().Contain("keycloak.example.com");
            result.AdminUrl.Should().Contain("keycloak.example.com");
            result.AdminUrl.Should().EndWith("/admin");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", originalKeycloakPassword);
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", originalPostgresPassword);
            Environment.SetEnvironmentVariable("KEYCLOAK_HOSTNAME", originalHostname);
        }
    }

    [Fact]
    public void AddMeAjudaAiKeycloakProduction_WithExposeHttpEndpointTrue_ShouldExposeHttpsEndpoint()
    {
        // Arrange
        var originalKeycloakPassword = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD");
        var originalPostgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        
        Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", "prod-admin");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "prod-db");

        try
        {
            // Act
            var result = _builder.AddMeAjudaAiKeycloakProduction(opts =>
            {
                opts.ExposeHttpEndpoint = true;
            });

            // Assert
            result.Should().NotBeNull();
            result.AuthUrl.Should().StartWith("https://localhost:");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", originalKeycloakPassword);
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", originalPostgresPassword);
        }
    }

    [Fact]
    public void AddMeAjudaAiKeycloakProduction_WithoutHostname_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var originalKeycloakPassword = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD");
        var originalPostgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        var originalHostname = Environment.GetEnvironmentVariable("KEYCLOAK_HOSTNAME");
        
        Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", "prod-admin");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "prod-db");
        Environment.SetEnvironmentVariable("KEYCLOAK_HOSTNAME", null);

        try
        {
            // Act & Assert
            var act = () => _builder.AddMeAjudaAiKeycloakProduction(opts =>
            {
                opts.ExposeHttpEndpoint = false;
                opts.Hostname = null;
            });
            
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*KEYCLOAK_HOSTNAME*obrigatório*");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", originalKeycloakPassword);
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", originalPostgresPassword);
            Environment.SetEnvironmentVariable("KEYCLOAK_HOSTNAME", originalHostname);
        }
    }

    [Fact]
    public void AddMeAjudaAiKeycloakProduction_WithCustomDatabaseSchema_ShouldApplyConfiguration()
    {
        // Arrange
        var originalKeycloakPassword = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD");
        var originalPostgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        
        Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", "prod-admin");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "prod-db");

        try
        {
            // Act
            var result = _builder.AddMeAjudaAiKeycloakProduction(opts =>
            {
                opts.DatabaseSchema = "production_identity";
                opts.Hostname = "keycloak.prod.com";
            });

            // Assert
            result.Should().NotBeNull();
            result.Keycloak.Should().NotBeNull();
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", originalKeycloakPassword);
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", originalPostgresPassword);
        }
    }

    [Fact]
    public void AddMeAjudaAiKeycloakProduction_WithNullImportRealm_ShouldNotConfigureImport()
    {
        // Arrange
        var originalKeycloakPassword = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD");
        var originalPostgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        
        Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", "prod-admin");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "prod-db");

        try
        {
            // Act
            var result = _builder.AddMeAjudaAiKeycloakProduction(opts =>
            {
                opts.ImportRealm = null;
                opts.Hostname = "keycloak.example.com";
            });

            // Assert
            result.Should().NotBeNull();
            result.Keycloak.Should().NotBeNull();
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", originalKeycloakPassword);
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", originalPostgresPassword);
        }
    }

    #endregion

    #region AddMeAjudaAiKeycloakTesting Tests

    [Fact]
    public void AddMeAjudaAiKeycloakTesting_WithDefaultOptions_ShouldReturnResult()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloakTesting();

        // Assert
        result.Should().NotBeNull();
        result.Keycloak.Should().NotBeNull();
        result.AuthUrl.Should().StartWith("http://localhost:");
        result.AdminUrl.Should().EndWith("/admin");
    }

    [Fact]
    public void AddMeAjudaAiKeycloakTesting_WithDefaultOptions_ShouldUseTestSchema()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloakTesting();

        // Assert
        result.Keycloak.Resource.Name.Should().Be("keycloak-test");
    }

    [Fact]
    public void AddMeAjudaAiKeycloakTesting_WithDefaultOptions_ShouldSetIsTestEnvironment()
    {
        // Arrange
        MeAjudaAiKeycloakOptions? capturedOptions = null;

        // Act
        var result = _builder.AddMeAjudaAiKeycloakTesting(opts =>
        {
            capturedOptions = opts;
        });

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.IsTestEnvironment.Should().BeTrue();
        capturedOptions.DatabaseSchema.Should().Be("identity_test");
        capturedOptions.AdminPassword.Should().Be("test123");
    }

    [Fact]
    public void AddMeAjudaAiKeycloakTesting_WithCustomOptions_ShouldApplyConfiguration()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloakTesting(opts =>
        {
            opts.DatabaseSchema = "custom_test_schema";
            opts.AdminPassword = "custom-test-pass";
        });

        // Assert
        result.Should().NotBeNull();
        result.Keycloak.Should().NotBeNull();
    }

    [Fact]
    public void AddMeAjudaAiKeycloakTesting_ShouldExposeHttpEndpoint()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloakTesting();

        // Assert
        result.Should().NotBeNull();
        result.AuthUrl.Should().StartWith("http://");
    }

    [Fact]
    public void AddMeAjudaAiKeycloakTesting_AuthUrl_ShouldUseCorrectPort()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloakTesting();

        // Assert
        result.AuthUrl.Should().MatchRegex(@"http://localhost:\d+");
    }

    [Fact]
    public void AddMeAjudaAiKeycloakTesting_AdminUrl_ShouldIncludeAdminPath()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloakTesting();

        // Assert
        result.AdminUrl.Should().Contain("/admin");
        result.AdminUrl.Should().StartWith(result.AuthUrl);
    }

    #endregion

    #region MeAjudaAiKeycloakResult Tests

    [Fact]
    public void MeAjudaAiKeycloakResult_ShouldHaveRequiredProperties()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloak();

        // Assert
        result.Keycloak.Should().NotBeNull();
        result.AuthUrl.Should().NotBeNullOrWhiteSpace();
        result.AdminUrl.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void MeAjudaAiKeycloakResult_AdminUrl_ShouldBeBasedOnAuthUrl()
    {
        // Act
        var result = _builder.AddMeAjudaAiKeycloak();

        // Assert
        result.AdminUrl.Should().StartWith(result.AuthUrl);
        result.AdminUrl.Should().EndWith("/admin");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void AddMeAjudaAiKeycloak_MultipleCalls_ShouldCreateDifferentResources()
    {
        // Act
        var result1 = _builder.AddMeAjudaAiKeycloak();
        
        var builder2 = DistributedApplication.CreateBuilder(new DistributedApplicationOptions
        {
            Args = Array.Empty<string>(),
            DisableDashboard = true
        });
        var result2 = builder2.AddMeAjudaAiKeycloak();

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Keycloak.Resource.Name.Should().Be("keycloak");
        result2.Keycloak.Resource.Name.Should().Be("keycloak");
    }

    [Fact]
    public void AllKeycloakMethods_ShouldReturnValidResults()
    {
        // Arrange
        var originalKeycloakPassword = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD");
        var originalPostgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        
        Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", "test-admin");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "test-db");

        try
        {
            // Act
            var devResult = _builder.AddMeAjudaAiKeycloak();
            
            var prodBuilder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions
            {
                Args = Array.Empty<string>(),
                DisableDashboard = true
            });
            var prodResult = prodBuilder.AddMeAjudaAiKeycloakProduction(opts =>
            {
                opts.Hostname = "keycloak.prod.com";
            });
            
            var testBuilder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions
            {
                Args = Array.Empty<string>(),
                DisableDashboard = true
            });
            var testResult = testBuilder.AddMeAjudaAiKeycloakTesting();

            // Assert
            devResult.Should().NotBeNull();
            devResult.AuthUrl.Should().StartWith("http://");
            
            prodResult.Should().NotBeNull();
            prodResult.AuthUrl.Should().Contain("keycloak.prod.com");
            
            testResult.Should().NotBeNull();
            testResult.Keycloak.Resource.Name.Should().Be("keycloak-test");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD", originalKeycloakPassword);
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", originalPostgresPassword);
        }
    }

    [Fact]
    public void KeycloakExtensions_ConfigurationCallback_ShouldBeInvoked()
    {
        // Arrange
        var callbackInvoked = false;
        MeAjudaAiKeycloakOptions? capturedOptions = null;

        // Act
        var result = _builder.AddMeAjudaAiKeycloak(opts =>
        {
            callbackInvoked = true;
            capturedOptions = opts;
            opts.DatabaseSchema = "callback_test";
        });

        // Assert
        callbackInvoked.Should().BeTrue();
        capturedOptions.Should().NotBeNull();
        capturedOptions!.DatabaseSchema.Should().Be("callback_test");
    }

    [Fact]
    public void KeycloakExtensions_WithNullCallback_ShouldNotThrow()
    {
        // Act
        var act = () => _builder.AddMeAjudaAiKeycloak(null);

        // Assert
        act.Should().NotThrow();
    }

    #endregion
}
