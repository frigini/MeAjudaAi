using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Integration.Tests.Database;

public class EmailTemplateRepositoryTests : IAsyncLifetime
{
    private readonly SimpleDatabaseFixture _databaseFixture;
    private readonly string _databaseName;
    private IServiceProvider _serviceProvider = null!;

    public EmailTemplateRepositoryTests()
    {
        _databaseFixture = new SimpleDatabaseFixture();
        _databaseName = $"test_emailtemplate_{Guid.NewGuid().ToString("N")[..8]}";
    }

    public async ValueTask InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();
        await _databaseFixture.CreateDatabaseAsync(_databaseName);

        var connectionString = _databaseFixture.GetConnectionString(_databaseName);

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
        services.AddScoped<DbContextEmailTemplateQueries>();

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
    public async Task EmailTemplateSchema_ShouldRejectTwoActiveBaseTemplates_WithSameKeyAndLanguage()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();

        var template1 = EmailTemplate.Create("key", "Subject", "Html", "Text", "pt-br");
        context.EmailTemplates.Add(template1);
        await context.SaveChangesAsync();

        var template2 = EmailTemplate.Create("key", "Subject", "Html", "Text", "pt-br");
        context.EmailTemplates.Add(template2);

        // Act & Assert
        await ((Func<Task>)(() => context.SaveChangesAsync())).Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task EmailTemplateSchema_ShouldAllowInactiveHistoricalVersions()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();

        var template1 = EmailTemplate.Create("key", "Subject", "Html", "Text", "pt-br");
        context.EmailTemplates.Add(template1);
        await context.SaveChangesAsync();
        
        template1.Deactivate();
        await context.SaveChangesAsync();

        var template2 = EmailTemplate.Create("key", "Subject", "Html", "Text", "pt-br");
        context.EmailTemplates.Add(template2);
        
        // Act & Assert
        await ((Func<Task>)(() => context.SaveChangesAsync())).Should().NotThrowAsync();
    }

    [Fact]
    public async Task EmailTemplateSchema_ShouldAllowDifferentOverrideKeys()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();

        var template1 = EmailTemplate.Create("key", "Subject", "Html", "Text", "pt-br", overrideKey: "over1");
        var template2 = EmailTemplate.Create("key", "Subject", "Html", "Text", "pt-br", overrideKey: "over2");
        
        context.EmailTemplates.Add(template1);
        context.EmailTemplates.Add(template2);
        
        // Act & Assert
        await ((Func<Task>)(() => context.SaveChangesAsync())).Should().NotThrowAsync();
    }
}
