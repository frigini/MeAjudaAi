using System.Net;
using FluentAssertions;
using MeAjudaAi.Shared.Tests.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Integration.Tests;

public class SimpleHealthTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Configurar uma connection string mock para health checks básicos
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5432;Database=meajudaai_mock;Username=postgres;Password=test;",
                    ["Postgres:ConnectionString"] = "Host=localhost;Port=5432;Database=meajudaai_mock;Username=postgres;Password=test;"
                });
            });
            builder.ConfigureServices(services =>
            {
                // Configurar serviços de teste básicos
                services.AddLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Warning);
                });

                // Configurar autenticação básica para evitar erros de DI
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, AspireTestAuthenticationHandler>("Test", options => { });
            });
        });

    [Fact]
    public async Task HealthEndpoint_ShouldReturnOk()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LivenessEndpoint_ShouldReturnOk()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReadinessEndpoint_ShouldReturnOk()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
