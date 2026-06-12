using FluentAssertions;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Shared.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Integration.Tests.Database;

public class DevelopmentDataSeederTests
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public DevelopmentDataSeederTests()
    {
        var services = new ServiceCollection();
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=meajudaai_test;Username=postgres;Password=postgres"
            }!)
            .Build();

        services.AddLogging();
        services.AddDbContext<CommunicationsDbContext>(options =>
            options.UseNpgsql(_configuration.GetConnectionString("DefaultConnection")));
        
        // Register necessary services for DevelopmentDataSeeder
        services.AddSingleton<IConfiguration>(_configuration);

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact(Skip = "Requires a running PostgreSQL instance which is not available in the CI environment.")]
    public async Task SeedCommunicationsAsync_ShouldBeIdempotent_AndCompatibleWithIndexes()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DevelopmentDataSeeder>>();
        
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

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
