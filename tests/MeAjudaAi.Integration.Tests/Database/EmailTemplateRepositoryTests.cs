using FluentAssertions;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MeAjudaAi.Integration.Tests.Database;

public class EmailTemplateRepositoryTests
{
    private readonly IServiceProvider _serviceProvider;

    public EmailTemplateRepositoryTests()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=meajudaai_test;Username=postgres;Password=postgres"
            }!)
            .Build();

        services.AddLogging();
        services.AddDbContext<CommunicationsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<DbContextEmailTemplateQueries>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact(Skip = "Requires a running PostgreSQL instance.")]
    public async Task EmailTemplateSchema_ShouldRejectTwoActiveBaseTemplates_WithSameKeyAndLanguage()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

        var template1 = EmailTemplate.Create("key", "Subject", "Html", "Text", "pt-br");
        var template2 = EmailTemplate.Create("key", "Subject", "Html", "Text", "pt-br");
        
        context.EmailTemplates.Add(template1);
        context.EmailTemplates.Add(template2);

        // Act & Assert
        await ((Func<Task>)(() => context.SaveChangesAsync())).Should().ThrowAsync<DbUpdateException>();
    }

    [Fact(Skip = "Requires a running PostgreSQL instance.")]
    public async Task EmailTemplateSchema_ShouldAllowInactiveHistoricalVersions()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

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

    [Fact(Skip = "Requires a running PostgreSQL instance.")]
    public async Task EmailTemplateSchema_ShouldAllowDifferentOverrideKeys()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommunicationsDbContext>();
        
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

        var template1 = EmailTemplate.Create("key", "Subject", "Html", "Text", "pt-br", overrideKey: "over1");
        var template2 = EmailTemplate.Create("key", "Subject", "Html", "Text", "pt-br", overrideKey: "over2");
        
        context.EmailTemplates.Add(template1);
        context.EmailTemplates.Add(template2);
        
        // Act & Assert
        await ((Func<Task>)(() => context.SaveChangesAsync())).Should().NotThrowAsync();
    }
}
