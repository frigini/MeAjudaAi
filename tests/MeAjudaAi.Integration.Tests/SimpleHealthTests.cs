using System.Net;
using FluentAssertions;
using MeAjudaAi.Shared.Tests.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;

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
                    ["Postgres:ConnectionString"] = "Host=localhost;Port=5432;Database=meajudaai_mock;Username=postgres;Password=test;",
                    ["INTEGRATION_TESTS"] = "true"
                });
            });
            builder.ConfigureServices(services =>
            {
                // Clear any existing loggers first to prevent conflicts
                services.RemoveAll<ILoggerFactory>();
                services.RemoveAll<ILoggerProvider>();

                // Configure Serilog properly for tests
                services.AddSerilog((serviceProvider, loggerConfig) =>
                {
                    loggerConfig
                        .MinimumLevel.Warning()
                        .WriteTo.Console()
                        .Enrich.FromLogContext();
                }, writeToProviders: false);

                // Configurar serviços de teste básicos
                services.AddLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Warning);
                });

                // Configurar autenticação básica para evitar erros de DI
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, AspireTestAuthenticationHandler>("Test", options => { });
                
                // Configure RabbitMqOptions for tests to prevent dependency resolution issues
                services.AddSingleton(new MeAjudaAi.Shared.Messaging.RabbitMq.RabbitMqOptions
                {
                    ConnectionString = "amqp://localhost",
                    DefaultQueueName = "test",
                    Host = "localhost",
                    Port = 5672,
                    Username = "guest",
                    Password = "guest",
                    VirtualHost = "/",
                    DomainQueues = new Dictionary<string, string> { ["Users"] = "users-events-test" }
                });
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
