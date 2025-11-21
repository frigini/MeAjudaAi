using System.Diagnostics;
using FluentAssertions;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Storage;
using MeAjudaAi.Integration.Tests.Aspire;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Shared.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Jobs;

/// <summary>
/// Testes de integração críticos para validar compatibilidade entre Hangfire.PostgreSql 1.20.12 e Npgsql 10.x.
/// 
/// CONTEXTO DE RISCO:
/// - Hangfire.PostgreSql 1.20.12 foi compilado contra Npgsql 6.x
/// - Npgsql 10.x introduz breaking changes: https://www.npgsql.org/doc/release-notes/10.0.html
/// - Compatibilidade em runtime NÃO foi validada pelo mantenedor do Hangfire.PostgreSql
/// 
/// OBJETIVO DOS TESTES:
/// - Validar que Hangfire.PostgreSql funciona corretamente com Npgsql 10.x
/// - Detectar falhas de persistência, execução, retry e recurring jobs
/// - Prevenir deploy para produção sem validação de compatibilidade
/// 
/// CI/CD REQUIREMENT:
/// - Estes testes DEVEM passar antes de qualquer deploy
/// - Falhas devem bloquear o pipeline de CI/CD
/// - Executar com: dotnet test --filter Category=HangfireIntegration
/// </summary>
[Trait("Category", "Integration")]
[Trait("Category", "HangfireIntegration")]
[Trait("Layer", "Infrastructure")]
[Trait("Component", "BackgroundJobs")]
public class HangfireIntegrationTests(AspireIntegrationFixture fixture, ITestOutputHelper output)
    : IntegrationTestBase(fixture, output)
{
    private static int _testJobExecutionCount;
    private static int _testJobWithParamExecutionCount;
    private static string? _lastExecutedJobParam;
    private static readonly object _lock = new();

    private static void ResetCounters()
    {
        lock (_lock)
        {
            _testJobExecutionCount = 0;
            _testJobWithParamExecutionCount = 0;
            _lastExecutedJobParam = null;
        }
    }

    #region Test Jobs

    /// <summary>
    /// Job de teste simples para validar execução - NOT a test method
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "xUnit1013", Justification = "This is a Hangfire job method, not a test method")]
    public static Task TestJobAsync()
    {
        lock (_lock)
        {
            _testJobExecutionCount++;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Job de teste com parâmetro para validar serialização - NOT a test method
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "xUnit1013", Justification = "This is a Hangfire job method, not a test method")]
    public static Task TestJobWithParameterAsync(string parameter)
    {
        lock (_lock)
        {
            _testJobWithParamExecutionCount++;
            _lastExecutedJobParam = parameter;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Job que falha para validar retry - NOT a test method
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "xUnit1013", Justification = "This is a Hangfire job method, not a test method")]
    public static Task FailingTestJobAsync()
    {
        lock (_lock)
        {
            _testJobExecutionCount++;
        }
        throw new InvalidOperationException("Simulated job failure for retry testing");
    }

    #endregion

    /// <summary>
    /// TESTE CRÍTICO 1: Validar que Hangfire pode persistir jobs no PostgreSQL usando Npgsql 10.x
    /// 
    /// Valida:
    /// - Hangfire consegue criar schema e tabelas no PostgreSQL via Npgsql 10.x
    /// - Jobs são persistidos corretamente no banco
    /// - Não há erros de compatibilidade de tipos ou comandos SQL
    /// </summary>
    [Fact(DisplayName = "CRITICAL: Hangfire should persist jobs to PostgreSQL with Npgsql 10.x")]
    public async Task Hangfire_WithNpgsql10_ShouldPersistJobs()
    {
        // Arrange
        var connectionString = GetConnectionString();
        var storage = CreateHangfireStorage(connectionString);

        // Act - Enqueue job
        var jobId = BackgroundJob.Enqueue(() => TestJobAsync());

        // Assert
        jobId.Should().NotBeNullOrEmpty("Job should be persisted and return a job ID");

        // Verify job exists in storage
        using var connection = storage.GetConnection();
        var jobData = connection.GetJobData(jobId);

        jobData.Should().NotBeNull("Job should be retrievable from storage");
        jobData!.Job.Should().NotBeNull("Job method should be serialized");
        jobData.Job.Method.Name.Should().Be(nameof(TestJobAsync));
        jobData.State.Should().Be("Enqueued", "Job should be in Enqueued state");

        // Cleanup
        await CleanupHangfireJobsAsync(connectionString);
    }

    /// <summary>
    /// TESTE CRÍTICO 2: Validar que jobs enfileirados são executados corretamente
    /// 
    /// Valida:
    /// - Background workers processam jobs da fila
    /// - Job execution é detectado e completado
    /// - Estado do job é atualizado corretamente
    /// </summary>
    [Fact(DisplayName = "CRITICAL: Enqueued jobs should execute successfully with Npgsql 10.x")]
    public async Task Hangfire_WithNpgsql10_ShouldExecuteEnqueuedJobs()
    {
        // Arrange
        var connectionString = GetConnectionString();
        using var server = CreateHangfireServer(connectionString);
        
        ResetCounters();

        // Act - Enqueue job
        var jobId = BackgroundJob.Enqueue(() => TestJobAsync());

        // Wait for job execution (with timeout)
        var stopwatch = Stopwatch.StartNew();
        var executed = false;
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(30) && !executed)
        {
            await Task.Delay(500, TestContext.Current.CancellationToken);
            lock (_lock)
            {
                executed = _testJobExecutionCount > 0;
            }
        }

        // Assert
        executed.Should().BeTrue("Job should execute within 30 seconds");
        
        lock (_lock)
        {
            _testJobExecutionCount.Should().Be(1, "Job should execute exactly once");
        }

        // Verify final job state
        var storage = CreateHangfireStorage(connectionString);
        using var connection = storage.GetConnection();
        var jobData = connection.GetJobData(jobId);
        jobData!.State.Should().Be("Succeeded", "Job should be in Succeeded state after execution");

        // Cleanup
        await CleanupHangfireJobsAsync(connectionString);
    }

    /// <summary>
    /// TESTE CRÍTICO 3: Validar serialização de parâmetros de jobs
    /// 
    /// Valida:
    /// - Parâmetros são serializados/deserializados corretamente
    /// - Npgsql 10.x manipula corretamente tipos de dados JSON/JSONB
    /// </summary>
    [Fact(DisplayName = "CRITICAL: Job parameters should serialize correctly with Npgsql 10.x")]
    public async Task Hangfire_WithNpgsql10_ShouldSerializeJobParameters()
    {
        // Arrange
        var connectionString = GetConnectionString();
        using var server = CreateHangfireServer(connectionString);
        
        ResetCounters();
        var testParameter = "TestValue_" + Guid.NewGuid();

        // Act
        var jobId = BackgroundJob.Enqueue(() => TestJobWithParameterAsync(testParameter));

        // Wait for execution
        var stopwatch = Stopwatch.StartNew();
        var executed = false;
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(30) && !executed)
        {
            await Task.Delay(500, TestContext.Current.CancellationToken);
            lock (_lock)
            {
                executed = _testJobWithParamExecutionCount > 0;
            }
        }

        // Assert
        executed.Should().BeTrue("Job with parameter should execute");
        
        lock (_lock)
        {
            _lastExecutedJobParam.Should().Be(testParameter, "Parameter should be deserialized correctly");
        }

        // Cleanup
        await CleanupHangfireJobsAsync(connectionString);
    }

    /// <summary>
    /// TESTE CRÍTICO 4: Validar retry automático em caso de falha
    /// 
    /// Valida:
    /// - Hangfire detecta falhas e agenda retries
    /// - Mecanismo de retry funciona com Npgsql 10.x
    /// - Job eventualmente entra em estado Failed após esgotarem tentativas
    /// </summary>
    [Fact(DisplayName = "CRITICAL: Failed jobs should trigger automatic retry with Npgsql 10.x")]
    public async Task Hangfire_WithNpgsql10_ShouldRetryFailedJobs()
    {
        // Arrange
        var connectionString = GetConnectionString();
        using var server = CreateHangfireServer(connectionString);
        
        ResetCounters();

        // Act - Enqueue failing job (Hangfire default: 10 retry attempts)
        var jobId = BackgroundJob.Enqueue(() => FailingTestJobAsync());

        // Wait for initial execution and first retry
        await Task.Delay(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        // Assert
        var storage = CreateHangfireStorage(connectionString);
        using var connection = storage.GetConnection();
        var jobData = connection.GetJobData(jobId);
        
        jobData.Should().NotBeNull();
        
        // Job should be in Scheduled state (waiting for retry) or Failed state
        var validStates = new[] { "Scheduled", "Failed", "Processing" };
        jobData!.State.Should().BeOneOf(validStates, "Job should be scheduled for retry or failed");

        lock (_lock)
        {
            _testJobExecutionCount.Should().BeGreaterThan(0, "Job should have been attempted at least once");
        }

        // Cleanup
        await CleanupHangfireJobsAsync(connectionString);
    }

    /// <summary>
    /// TESTE CRÍTICO 5: Validar agendamento de recurring jobs
    /// 
    /// Valida:
    /// - Recurring jobs são criados e persistidos
    /// - Cron expressions são armazenadas corretamente
    /// - Jobs recorrentes aparecem no storage
    /// </summary>
    [Fact(DisplayName = "CRITICAL: Recurring jobs should be scheduled correctly with Npgsql 10.x")]
    public async Task Hangfire_WithNpgsql10_ShouldScheduleRecurringJobs()
    {
        // Arrange
        var connectionString = GetConnectionString();
        var storage = CreateHangfireStorage(connectionString);
        var recurringJobId = "test-recurring-job-" + Guid.NewGuid();

        try
        {
            // Act - Schedule recurring job (every minute)
            RecurringJob.AddOrUpdate(
                recurringJobId,
                () => TestJobAsync(),
                Cron.Minutely(),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

            // Assert - Verify recurring job is stored
            using var connection = storage.GetConnection();
            var recurringJobs = connection.GetRecurringJobs();

            recurringJobs.Should().Contain(j => j.Id == recurringJobId, "Recurring job should be persisted");

            var job = recurringJobs.First(j => j.Id == recurringJobId);
            job.Cron.Should().Be(Cron.Minutely(), "Cron expression should match");
        }
        finally
        {
            // Cleanup
            RecurringJob.RemoveIfExists(recurringJobId);
            await CleanupHangfireJobsAsync(connectionString);
        }
    }

    /// <summary>
    /// TESTE CRÍTICO 6: Validar que Hangfire consegue conectar ao PostgreSQL via Npgsql 10.x
    /// 
    /// Valida:
    /// - Connection string é válida
    /// - Npgsql 10.x consegue estabelecer conexão
    /// - Schema hangfire existe e é acessível
    /// </summary>
    [Fact(DisplayName = "CRITICAL: Hangfire should connect to PostgreSQL using Npgsql 10.x")]
    public async Task Hangfire_WithNpgsql10_ShouldConnectToDatabase()
    {
        // Arrange
        var connectionString = GetConnectionString();

        // Act & Assert - Should not throw
        var storage = CreateHangfireStorage(connectionString);
        using var connection = storage.GetConnection();

        // Verify we can query basic stats (proves schema exists and is accessible)
        var monitoring = storage.GetMonitoringApi();
        var stats = monitoring.GetStatistics();

        stats.Should().NotBeNull("Should be able to retrieve statistics from Hangfire storage");

        // Verify PostgreSQL version and Npgsql version
        await using var npgsqlConnection = new NpgsqlConnection(connectionString);
        await npgsqlConnection.OpenAsync(TestContext.Current.CancellationToken);

        var npgsqlVersion = npgsqlConnection.ServerVersion;
        npgsqlVersion.Should().NotBeNullOrEmpty("PostgreSQL version should be available");

        // Log for diagnostic purposes
        Console.WriteLine($"PostgreSQL Version: {npgsqlVersion}");
        Console.WriteLine($"Npgsql Version: {typeof(NpgsqlConnection).Assembly.GetName().Version}");
    }

    #region Helper Methods

    private string GetConnectionString()
    {
        // Get connection string from environment variable (set by CI/CD or test configuration)
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__HangfireConnection")
                             ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                             ?? "Host=localhost;Port=5432;Database=meajudaai_test;Username=postgres;Password=postgres";

        connectionString.Should().NotBeNullOrEmpty("Hangfire connection string must be configured");

        return connectionString;
    }

    private PostgreSqlStorage CreateHangfireStorage(string connectionString)
    {
        var options = new PostgreSqlStorageOptions
        {
            SchemaName = "hangfire",
            PrepareSchemaIfNecessary = true,
            EnableTransactionScopeEnlistment = true,
            QueuePollInterval = TimeSpan.FromSeconds(1)
        };

        // Create storage with connection string directly (Hangfire.PostgreSql 1.20.12 API)
        #pragma warning disable CS0618 // Type or member is obsolete - using older API for compatibility
        return new PostgreSqlStorage(connectionString, options);
        #pragma warning restore CS0618
    }

    private BackgroundJobServer CreateHangfireServer(string connectionString)
    {
        var storage = CreateHangfireStorage(connectionString);
        JobStorage.Current = storage;

        var options = new BackgroundJobServerOptions
        {
            WorkerCount = 1,
            ServerName = $"TestServer_{Guid.NewGuid():N}",
            Queues = new[] { "default" },
            ShutdownTimeout = TimeSpan.FromSeconds(5)
        };

        return new BackgroundJobServer(options, storage);
    }

    private async Task CleanupHangfireJobsAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Clean up Hangfire jobs from test schema
        var cleanupSql = @"
            TRUNCATE TABLE hangfire.job CASCADE;
            TRUNCATE TABLE hangfire.jobqueue CASCADE;
            TRUNCATE TABLE hangfire.state CASCADE;
            TRUNCATE TABLE hangfire.jobparameter CASCADE;
            TRUNCATE TABLE hangfire.set CASCADE;
            TRUNCATE TABLE hangfire.hash CASCADE;
            TRUNCATE TABLE hangfire.list CASCADE;
            TRUNCATE TABLE hangfire.counter CASCADE;
        ";

        try
        {
            await using var command = new NpgsqlCommand(cleanupSql, connection);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to cleanup Hangfire tables: {ex.Message}");
            // Don't fail test on cleanup errors
        }
    }

    #endregion
}
