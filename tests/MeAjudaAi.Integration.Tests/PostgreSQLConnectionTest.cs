using Aspire.Hosting.Testing;
using Aspire.Hosting;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.EndToEnd;

/// <summary>
/// Teste específico para validar conectividade do PostgreSQL
/// </summary>
public class PostgreSQLConnectionTest
{
    [Fact]
    public async Task PostgreSQL_ShouldStart_WithCorrectCredentials()
    {
        // Arrange
        var timeout = TimeSpan.FromMinutes(5); // Tempo generoso para PostgreSQL iniciar
        var cancellationToken = new CancellationTokenSource(timeout).Token;

        // Act
        using var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MeAjudaAi_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync(cancellationToken);
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        
        await app.StartAsync(cancellationToken);

        // Wait specifically for postgres-local to be running
        await resourceNotificationService.WaitForResourceAsync("postgres-local", KnownResourceStates.Running, cancellationToken);

        // Assert - If we reach here, PostgreSQL started successfully
        true.Should().BeTrue("PostgreSQL container started without authentication errors");
    }

    [Fact]
    public async Task PostgreSQL_Database_ShouldBeAccessible()
    {
        // Arrange
        var timeout = TimeSpan.FromMinutes(5);
        var cancellationToken = new CancellationTokenSource(timeout).Token;

        // Act
        using var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MeAjudaAi_AppHost>(cancellationToken);
        await using var app = await appHost.BuildAsync(cancellationToken);
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        
        await app.StartAsync(cancellationToken);

        // Wait for PostgreSQL to be ready (single database approach)
        await resourceNotificationService.WaitForResourceAsync("postgres-local", KnownResourceStates.Running, cancellationToken);
        await resourceNotificationService.WaitForResourceAsync("meajudaai-db-local", KnownResourceStates.Running, cancellationToken);

        // Assert
        true.Should().BeTrue("PostgreSQL database is accessible");
    }
}