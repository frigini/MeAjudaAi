using MeAjudaAi.Modules.Users.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.Tests.Unit.API.Extensions;

/// <summary>
/// Testes unitários específicos dos métodos de extensão da API
/// Foca em validação de parâmetros e comportamentos unitários
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "API")]
[Trait("Component", "Extensions")]
public class APIExtensionsTests
{
    [Fact]
    public void AddUsersModule_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = "Host=localhost;Database=test;Username=user;Password=pass",
                ["Database:EnableSchemaIsolation"] = "false"
            })
            .Build();

        // Act
        var result = services.AddUsersModule(configuration);

        // Assert
        result.Should().BeSameAs(services);
        services.Should().NotBeEmpty();

        // Verificar se serviços foram registrados (teste estrutural)
        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.Should().NotBeNull();
    }

    [Fact]
    public async Task AddUsersModuleWithSchemaIsolationAsync_WithSchemaIsolationDisabled_ShouldAddServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = "Host=localhost;Database=test;Username=user;Password=pass",
                ["Database:EnableSchemaIsolation"] = "false"
            })
            .Build();

        // Act
        var result = await services.AddUsersModuleWithSchemaIsolationAsync(configuration);

        // Assert
        result.Should().BeSameAs(services);
        services.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddUsersModuleWithSchemaIsolationAsync_WithNullPasswords_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = "Host=localhost;Database=test;Username=user;Password=pass",
                ["Database:EnableSchemaIsolation"] = "false"
            })
            .Build();

        // Act & Assert
        var act = async () => await services.AddUsersModuleWithSchemaIsolationAsync(
            configuration,
            usersRolePassword: null,
            appRolePassword: null);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void UseUsersModule_ShouldConfigureApplication()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddRouting();

        using var app = builder.Build();

        // Act
        var result = app.UseUsersModule();

        // Assert
        result.Should().BeSameAs(app);
        // Verificação estrutural - se chegou até aqui sem exceção, o método funcionou
    }

    [Fact]
    public void AddUsersModule_WithNullConfiguration_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration configuration = null!;

        // Act & Assert
        var act = () => services.AddUsersModule(configuration);
        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void AddUsersModule_WithNullServices_ShouldThrow()
    {
        // Arrange
        IServiceCollection services = null!;
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        var act = () => services.AddUsersModule(configuration);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task AddUsersModuleWithSchemaIsolationAsync_WithNullServices_ShouldThrow()
    {
        // Arrange
        IServiceCollection services = null!;
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        var act = async () => await services.AddUsersModuleWithSchemaIsolationAsync(configuration);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AddUsersModuleWithSchemaIsolationAsync_WithNullConfiguration_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration configuration = null!;

        // Act & Assert
        var act = async () => await services.AddUsersModuleWithSchemaIsolationAsync(configuration);
        await act.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public void UseUsersModule_WithNullApp_ShouldThrow()
    {
        // Arrange
        WebApplication app = null!;

        // Act & Assert
        var act = () => app.UseUsersModule();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddUsersModule_WithValidConfiguration_ShouldReturnSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configData = new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = "Server=localhost;Database=TestDb;Trusted_Connection=true;",
            ["Cache:RedisConnectionString"] = "localhost:6379"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var result = services.AddUsersModule(configuration);

        // Assert
        result.Should().BeSameAs(services);

        // Verificar que pelo menos alguns serviços foram registrados
        services.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AddUsersModule_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = "Host=localhost;Database=test;Username=user;Password=pass"
            })
            .Build();

        // Act & Assert
        var act = () =>
        {
            services.AddUsersModule(configuration);
            services.AddUsersModule(configuration);
        };

        act.Should().NotThrow();
    }
}