using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
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
        services.AddSingleton<IConfiguration>(_configuration);

        services.AddDbContext<ServiceCatalogsDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly("MeAjudaAi.Modules.ServiceCatalogs.Infrastructure");
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "service_catalogs");
            });
            options.UseSnakeCaseNamingConvention();
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddDbContext<LocationsDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly("MeAjudaAi.Modules.Locations.Infrastructure");
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "locations");
            });
            options.UseSnakeCaseNamingConvention();
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddDbContext<ProvidersDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly("MeAjudaAi.Modules.Providers.Infrastructure");
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "providers");
            });
            options.UseSnakeCaseNamingConvention();
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

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

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();

        var serviceCatalogsDb = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
        serviceCatalogsDb.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));
        await serviceCatalogsDb.Database.MigrateAsync();

        var locationsDb = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
        locationsDb.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));
        await locationsDb.Database.MigrateAsync();

        var providersDb = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        providersDb.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));
        await providersDb.Database.MigrateAsync();

        var communicationsDb = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        communicationsDb.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));
        await communicationsDb.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_databaseFixture != null)
            await _databaseFixture.DropDatabaseAsync(_databaseName);
    }

    private async Task<DevelopmentDataSeeder> SeedAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DevelopmentDataSeeder>>();
        var seeder = new DevelopmentDataSeeder(_serviceProvider, logger);
        await seeder.ForceSeedAsync(CancellationToken.None);
        return seeder;
    }

    [Fact]
    public async Task SeedServiceCatalogs_ShouldCreateCategories()
    {
        await SeedAsync();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();

        var count = await context.ServiceCategories.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(6, "seeder must create at least 6 service categories");
    }

    [Fact]
    public async Task SeedServiceCatalogs_ShouldCreateServices()
    {
        await SeedAsync();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();

        var count = await context.Services.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(12, "seeder must create at least 12 services");
    }

    [Fact]
    public async Task SeedServiceCatalogs_AllServicesLinkedToCategories()
    {
        await SeedAsync();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();

        var orphanCount = await context.Services
            .Where(s => !context.ServiceCategories.Any(c => c.Id == s.CategoryId))
            .CountAsync();

        orphanCount.Should().Be(0, "all services must reference an existing category");
    }

    [Fact]
    public async Task SeedLocations_ShouldCreateAllowedCities()
    {
        await SeedAsync();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();

        var count = await context.AllowedCities.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(10, "seeder must create at least 10 allowed cities");
    }

    [Fact]
    public async Task SeedLocations_ShouldIncludeLinhares()
    {
        await SeedAsync();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();

        var linhares = await context.AllowedCities
            .FirstOrDefaultAsync(c => c.CityName == "Linhares" && c.StateSigla == "ES");

        linhares.Should().NotBeNull("Linhares must be seeded as an allowed city");
    }

    [Fact]
    public async Task SeedProviders_ShouldCreateProviders()
    {
        await SeedAsync();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();

        var count = await context.Providers.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(10, "seeder must create at least 10 providers");
    }

    [Fact]
    public async Task SeedCommunications_ShouldCreateEmailTemplates()
    {
        await SeedAsync();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();

        var count = await context.EmailTemplates.CountAsync();
        count.Should().BeGreaterThan(0, "seeder must create email templates");
    }

    [Fact]
    public async Task SeedAllModules_ShouldBeIdempotent()
    {
        // First run
        await SeedAsync();

        using var scope = _serviceProvider.CreateScope();
        var serviceCatalogs = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
        var locations = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
        var providers = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        var communications = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();

        var categoriesAfterFirst = await serviceCatalogs.ServiceCategories.CountAsync();
        var servicesAfterFirst = await serviceCatalogs.Services.CountAsync();
        var citiesAfterFirst = await locations.AllowedCities.CountAsync();
        var providersAfterFirst = await providers.Providers.CountAsync();
        var templatesAfterFirst = await communications.EmailTemplates.CountAsync();

        // Second run (idempotency check)
        Func<Task> secondRun = () => SeedAsync();
        await secondRun.Should().NotThrowAsync();

        (await serviceCatalogs.ServiceCategories.CountAsync()).Should().Be(categoriesAfterFirst);
        (await serviceCatalogs.Services.CountAsync()).Should().Be(servicesAfterFirst);
        (await locations.AllowedCities.CountAsync()).Should().Be(citiesAfterFirst);
        (await providers.Providers.CountAsync()).Should().Be(providersAfterFirst);
        (await communications.EmailTemplates.CountAsync()).Should().Be(templatesAfterFirst);
    }
}
