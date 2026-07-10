using MeAjudaAi.Shared.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Architecture.Tests.Database;

/// <summary>
/// Verifica que o modelo de domínio está corretamente mapeado no EF Core para todos os módulos.
/// Previne erros de mapeamento que só seriam descobertos em runtime.
/// </summary>
[Trait("Category", "Architecture")]
public class SchemaMappingTests
{
    private static ServiceProvider CreateServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = DatabaseConstants.DefaultTestConnectionString,
                ["Stripe:ApiKey"] = "sk_test_dummy",
                ["Messaging:Enabled"] = "false"
            })
            .Build();

        var hostingEnv = Mock.Of<IHostEnvironment>(e => e.EnvironmentName == "Testing");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        // Register PostgresOptions (required by some modules' persistence registration)
        services.AddSingleton(new MeAjudaAi.Shared.Database.PostgresOptions
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection") ?? DatabaseConstants.DefaultTestConnectionString
        });

        // Register Database Monitoring services (required by DatabaseExtensions)
        services.AddSingleton<MeAjudaAi.Shared.Database.DatabaseMetrics>();
        services.AddSingleton<MeAjudaAi.Shared.Database.DatabaseMetricsInterceptor>();

        // Register all modules
        MeAjudaAi.Modules.Users.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Providers.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Communications.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Bookings.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Documents.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Locations.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Payments.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Ratings.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.SearchProviders.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);

        return services.BuildServiceProvider();
    }

    [Fact]
    public void User_DeviceToken_ShouldBeMapped()
    {
        // Arrange
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>();
        var entityType = dbContext.Model.FindEntityType(typeof(MeAjudaAi.Modules.Users.Domain.Entities.User));

        // Act
        var property = entityType?.FindProperty("DeviceToken");

        // Assert
        entityType.Should().NotBeNull("User entity should be registered in DbContext");
        property.Should().NotBeNull("DeviceToken property should be mapped");
        property!.GetColumnName().Should().Be("device_token");
        property.IsNullable.Should().BeTrue("DeviceToken should be nullable");
    }

    [Fact]
    public void Provider_DeviceToken_ShouldBeMapped()
    {
        // Arrange
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>();
        var entityType = dbContext.Model.FindEntityType(typeof(MeAjudaAi.Modules.Providers.Domain.Entities.Provider));

        // Act
        var property = entityType?.FindProperty("DeviceToken");

        // Assert
        entityType.Should().NotBeNull("Provider entity should be registered in DbContext");
        property.Should().NotBeNull("DeviceToken property should be mapped");
        property!.GetColumnName().Should().Be("device_token");
        property.IsNullable.Should().BeTrue("DeviceToken should be nullable");
    }

    [Fact]
    public void EmailTemplate_Version_ShouldBeMapped()
    {
        // Arrange
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.Communications.Infrastructure.Persistence.CommunicationsDbContext>();
        var entityType = dbContext.Model.FindEntityType(typeof(MeAjudaAi.Modules.Communications.Domain.Entities.EmailTemplate));

        // Act
        var property = entityType?.FindProperty("Version");

        // Assert
        entityType.Should().NotBeNull("EmailTemplate entity should be registered in DbContext");
        property.Should().NotBeNull("Version property should be mapped");
        property!.GetColumnName().Should().Be("version");
    }

    [Fact]
    public void EmailTemplate_ShouldHaveUniqueIndexPerActiveVersion()
    {
        // Arrange
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.Communications.Infrastructure.Persistence.CommunicationsDbContext>();
        var entityType = dbContext.Model.FindEntityType(typeof(MeAjudaAi.Modules.Communications.Domain.Entities.EmailTemplate));

        // Act
        var indexes = entityType!.GetIndexes();
        var uniqueIndex = indexes.FirstOrDefault(i =>
            i.IsUnique &&
            i.Properties.Any(p => p.Name == "TemplateKey") &&
            i.Properties.Any(p => p.Name == "Language") &&
            i.Properties.Any(p => p.Name == "OverrideKey"));

        // Assert
        entityType.Should().NotBeNull();
        uniqueIndex.Should().NotBeNull("Should have a unique index on (TemplateKey, Language, OverrideKey)");
        uniqueIndex!.GetFilter().Should().Contain("is_active", "Unique index should filter by active status");
    }

    [Fact]
    public void Communications_Tables_ShouldExist()
    {
        // Arrange
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.Communications.Infrastructure.Persistence.CommunicationsDbContext>();
        var model = dbContext.Model;

        // Act
        var emailTemplateEntity = model.FindEntityType(typeof(MeAjudaAi.Modules.Communications.Domain.Entities.EmailTemplate));
        var communicationLogEntity = model.FindEntityType(typeof(MeAjudaAi.Modules.Communications.Domain.Entities.CommunicationLog));
        var outboxMessageEntity = model.FindEntityType(typeof(MeAjudaAi.Modules.Communications.Domain.Entities.OutboxMessage));

        // Assert
        emailTemplateEntity.Should().NotBeNull("EmailTemplate should be mapped");
        emailTemplateEntity!.GetTableName().Should().Be("email_templates");
        emailTemplateEntity.GetSchema().Should().Be("communications");

        communicationLogEntity.Should().NotBeNull("CommunicationLog should be mapped");
        communicationLogEntity!.GetTableName().Should().Be("communication_logs");
        communicationLogEntity.GetSchema().Should().Be("communications");

        outboxMessageEntity.Should().NotBeNull("OutboxMessage should be mapped");
        outboxMessageEntity!.GetTableName().Should().Be("outbox_messages");
        outboxMessageEntity.GetSchema().Should().Be("communications");
    }

    [Fact]
    public void Bookings_Tables_ShouldExist()
    {
        // Arrange
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.Bookings.Infrastructure.Persistence.BookingsDbContext>();

        // Act
        var bookingEntity = dbContext.Model.FindEntityType(typeof(MeAjudaAi.Modules.Bookings.Domain.Entities.Booking));

        // Assert
        bookingEntity.Should().NotBeNull("Booking should be mapped");
        bookingEntity!.GetTableName().Should().Be("bookings");
        bookingEntity.GetSchema().Should().Be("bookings");
    }

    [Fact]
    public void Ratings_Tables_ShouldExist()
    {
        // Arrange
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.RatingsDbContext>();

        // Act
        var reviewEntity = dbContext.Model.FindEntityType(typeof(MeAjudaAi.Modules.Ratings.Domain.Entities.Review));

        // Assert
        reviewEntity.Should().NotBeNull("Review should be mapped");
        reviewEntity!.GetTableName().Should().Be("reviews");
        reviewEntity.GetSchema().Should().Be("ratings");
    }

    [Fact]
    public void Payments_Tables_ShouldExist()
    {
        // Arrange
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.Payments.Infrastructure.Persistence.PaymentsDbContext>();

        // Act
        var subscriptionEntity = dbContext.Model.FindEntityType(typeof(MeAjudaAi.Modules.Payments.Domain.Entities.Subscription));

        // Assert
        subscriptionEntity.Should().NotBeNull("Subscription should be mapped");
        subscriptionEntity!.GetTableName().Should().Be("subscriptions");
        subscriptionEntity.GetSchema().Should().Be("payments");
    }

    [Fact]
    public void SearchProviders_Tables_ShouldExist()
    {
        // Arrange
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.SearchProvidersDbContext>();

        // Act
        var searchableProviderEntity = dbContext.Model.FindEntityType(typeof(MeAjudaAi.Modules.SearchProviders.Domain.Entities.SearchableProvider));

        // Assert
        searchableProviderEntity.Should().NotBeNull("SearchableProvider should be mapped");
        searchableProviderEntity!.GetTableName().Should().Be("searchable_providers");
        searchableProviderEntity.GetSchema().Should().Be("search_providers");
    }
}
