using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Shared.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Integration.Tests.Database;

public class DevelopmentDataSeederTests : IAsyncLifetime
{
    private readonly SimpleDatabaseFixture _databaseFixture;
    private readonly string _databaseName;
    private IConfiguration _configuration = null!;
    private IServiceProvider _serviceProvider = null!;

    public DevelopmentDataSeederTests()
    {
        _databaseFixture = new SimpleDatabaseFixture();
        _databaseName = $"test_dataseeder_{Guid.NewGuid().ToString("N")[..8]}";
    }

    public async ValueTask InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();
        await _databaseFixture.CreateDatabaseAsync(_databaseName);

        var connectionString = _databaseFixture.GetConnectionString(_databaseName);

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            }!)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<CommunicationsDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly("MeAjudaAi.Modules.Communications.Infrastructure");
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "communications");
            });
            options.UseSnakeCaseNamingConvention();
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });
        services.AddSingleton<IConfiguration>(_configuration);

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));
        await context.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_databaseFixture != null)
            await _databaseFixture.DropDatabaseAsync(_databaseName);
    }

    [Fact]
    public async Task SeedCommunicationsAsync_ShouldBeIdempotent_AndCompatibleWithIndexes()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DevelopmentDataSeeder>>();

        var seeder = new DevelopmentDataSeeder(_serviceProvider, logger);

        // Act & Assert
        // First run
        await seeder.ForceSeedAsync(CancellationToken.None);
        var countAfterFirstRun = await context.EmailTemplates.CountAsync();
        countAfterFirstRun.Should().BeGreaterThan(0);

        // Second run (Idempotency check)
        Func<Task> secondRun = () => seeder.ForceSeedAsync(CancellationToken.None);
        await secondRun.Should().NotThrowAsync();
        
        var countAfterSecondRun = await context.EmailTemplates.CountAsync();
        countAfterSecondRun.Should().Be(countAfterFirstRun);
    }
}
