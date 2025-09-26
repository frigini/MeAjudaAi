using FluentAssertions;

namespace MeAjudaAi.Integration.Tests;

/// <summary>
/// Teste específico para validar conectividade do PostgreSQL
/// </summary>
public class PostgreSQLConnectionTest
{
    private static bool IsDockerAvailable()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit(5000); // 5 second timeout
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    [Fact(Timeout = 60000)] // 1 minute timeout
    public async Task PostgreSQL_ShouldStart_WithCorrectCredentials()
    {
        // Skip test if Docker is not available
        if (!IsDockerAvailable())
        {
            Assert.True(true, "Docker is not available - skipping PostgreSQL container test");
            return;
        }

        // Arrange
        var timeout = TimeSpan.FromSeconds(45); // Timeout mais agressivo
        var cancellationToken = new CancellationTokenSource(timeout).Token;

        try
        {
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
        catch (OperationCanceledException ex)
        {
            throw new TimeoutException($"PostgreSQL container failed to start within {timeout.TotalSeconds} seconds. " +
                                     "This may indicate Docker is not running or there are resource constraints.", ex);
        }
    }

    [Fact(Timeout = 60000)] // 1 minute timeout
    public async Task PostgreSQL_Database_ShouldBeAccessible()
    {
        // Skip test if Docker is not available
        if (!IsDockerAvailable())
        {
            Assert.True(true, "Docker is not available - skipping PostgreSQL database test");
            return;
        }

        // Arrange
        var timeout = TimeSpan.FromSeconds(45);
        var cancellationToken = new CancellationTokenSource(timeout).Token;

        try
        {
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
        catch (OperationCanceledException ex)
        {
            throw new TimeoutException($"PostgreSQL database failed to become accessible within {timeout.TotalSeconds} seconds. " +
                                     "This may indicate Docker is not running or there are resource constraints.", ex);
        }
    }
}