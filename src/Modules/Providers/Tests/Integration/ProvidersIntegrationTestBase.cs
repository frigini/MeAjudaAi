using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Tests.Infrastructure;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Integration;

/// <summary>
/// Classe base para testes de integração específicos do módulo Providers.
/// Usa isolamento completo com database único por classe de teste.
/// </summary>
public abstract class ProvidersIntegrationTestBase : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private ServiceProvider? _serviceProvider;
    private readonly string _testClassId;

    protected ProvidersIntegrationTestBase()
    {
        _testClassId = $"{GetType().Name}_{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Configurações padrão para testes do módulo Providers
    /// </summary>
    protected TestInfrastructureOptions GetTestOptions()
    {
        return new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                DatabaseName = $"providers_test_{_testClassId}",
                Username = "test_user",
                Password = "test_password",
                Schema = "providers",
                UseInMemoryDatabase = false
            },
            Cache = new TestCacheOptions
            {
                Enabled = false
            },
            ExternalServices = new TestExternalServicesOptions
            {
                UseKeycloakMock = true,
                UseMessageBusMock = true
            }
        };
    }

    /// <summary>
    /// Inicialização executada antes de cada classe de teste
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        // Criar container PostgreSQL isolado para esta classe de teste
        var options = GetTestOptions();

        _container = new PostgreSqlBuilder(options.Database.PostgresImage)
            .WithDatabase(options.Database.DatabaseName)
            .WithUsername(options.Database.Username)
            .WithPassword(options.Database.Password)
            .Build();

        await _container.StartAsync();

        // Configurar serviços com container isolado
        var services = new ServiceCollection();

        // Registrar o container específico
        services.AddSingleton(_container);

        // Configurar logging otimizado
        services.AddLogging(builder =>
        {
            builder.ConfigureTestLogging();
        });

        // Configurar serviços específicos do módulo
        ConfigureModuleServices(services, options);

        _serviceProvider = services.BuildServiceProvider();

        // Inicializar banco de dados
        await InitializeDatabaseAsync();
    }

    /// <summary>
    /// Configura serviços específicos do módulo Providers
    /// </summary>
    private void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddProvidersTestInfrastructure(options);
    }

    /// <summary>
    /// Inicializa o banco de dados isolado
    /// </summary>
    private async Task InitializeDatabaseAsync()
    {
        var dbContext = _serviceProvider!.GetRequiredService<ProvidersDbContext>();

        // Criar banco e esquema sem executar migrations
        await dbContext.Database.EnsureCreatedAsync();

        // Verificar isolamento
        var count = await dbContext.Providers.CountAsync();
        if (count > 0)
        {
            throw new InvalidOperationException($"Database isolation failed for '{GetTestOptions().Database.DatabaseName}': found {count} existing providers in new database");
        }
    }

    /// <summary>
    /// Cria um provedor para teste e persiste no banco de dados
    /// </summary>
    protected async Task<Provider> CreateProviderAsync(
        Guid userId,
        string name,
        EProviderType type,
        BusinessProfile businessProfile,
        CancellationToken cancellationToken = default)
    {
        var provider = new Provider(userId, name, type, businessProfile);

        // Obter contexto
        var dbContext = GetService<ProvidersDbContext>();

        await dbContext.Providers.AddAsync(provider, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return provider;
    }

    /// <summary>
    /// Cria um perfil empresarial padrão para testes
    /// </summary>
    protected static BusinessProfile CreateTestBusinessProfile(string email = "test@example.com", string phone = "+5511999999999")
    {
        var contactInfo = new ContactInfo(email, phone);
        var address = new Address("Test Street", "123", "Test Neighborhood", "Test City", "Test State", "12345-678", "Brazil");

        return new BusinessProfile("Test Company Legal Name", contactInfo, address, "Test Company Fantasy", "Test company description");
    }

    /// <summary>
    /// Limpa dados das tabelas para isolamento entre testes
    /// Usando banco isolado, cleanup é mais simples e confiável
    /// </summary>
    protected async Task CleanupDatabase()
    {
        var dbContext = GetService<ProvidersDbContext>();
        var schema = GetTestOptions().Database.Schema;

        try
        {
            // Com banco isolado, podemos usar TRUNCATE com segurança
#pragma warning disable EF1002 // Risk of SQL injection - schema comes from test configuration, not user input
            await dbContext.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {schema}.providers CASCADE");
#pragma warning restore EF1002
        }
        catch (Exception ex)
        {
            // Fallback para DELETE se TRUNCATE falhar
            var logger = GetService<ILogger<ProvidersIntegrationTestBase>>();
            logger.LogWarning(ex, "TRUNCATE failed: {Message}. Using DELETE fallback...", ex.Message);

#pragma warning disable EF1002 // Risk of SQL injection - schema comes from test configuration, not user input
            await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM {schema}.qualification");
            await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM {schema}.document");
            await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM {schema}.providers");
#pragma warning restore EF1002
        }

        // Verificar se limpeza foi bem-sucedida
        var remainingCount = await dbContext.Providers.CountAsync();
        if (remainingCount > 0)
        {
            throw new InvalidOperationException($"Database cleanup failed: {remainingCount} providers remain");
        }
    }

    /// <summary>
    /// Força limpeza mais agressiva do banco de dados
    /// Com isolamento completo, é mais simples e confiável
    /// </summary>
    protected async Task ForceCleanDatabase()
    {
        var dbContext = GetService<ProvidersDbContext>();
        var schema = GetTestOptions().Database.Schema;

        try
        {
            // Estratégia 1: TRUNCATE CASCADE
#pragma warning disable EF1002 // Risk of SQL injection - schema comes from test configuration, not user input
            await dbContext.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {schema}.providers CASCADE");
#pragma warning restore EF1002
            return;
        }
        catch (Exception ex)
        {
            var logger = GetService<ILogger<ProvidersIntegrationTestBase>>();
            logger.LogWarning(ex, "TRUNCATE failed: {Message}. Trying DELETE...", ex.Message);
        }

        try
        {
            // Estratégia 2: DELETE em ordem reversa
#pragma warning disable EF1002 // Risk of SQL injection - schema comes from test configuration, not user input
            await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM {schema}.qualification");
            await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM {schema}.document");
            await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM {schema}.providers");
#pragma warning restore EF1002
            return;
        }
        catch (Exception ex)
        {
            var logger = GetService<ILogger<ProvidersIntegrationTestBase>>();
            logger.LogError(ex, "DELETE failed: {Message}. Recreating database...", ex.Message);
        }

        // Estratégia 3: Recriar database
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Limpeza executada após cada classe de teste
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
        }

        if (_container != null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// Acesso direto ao contexto de banco de dados do módulo Providers
    /// </summary>
    protected ProvidersDbContext DbContext => GetService<ProvidersDbContext>();

    /// <summary>
    /// Obtém um serviço do provider isolado
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("Service provider not initialized");
        return _serviceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Obtém um serviço de um escopo específico
    /// </summary>
    protected T GetScopedService<T>(IServiceScope scope) where T : notnull
    {
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Cria um escopo de serviços para o teste
    /// </summary>
    protected IServiceScope CreateScope()
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("Service provider not initialized");
        return _serviceProvider.CreateScope();
    }
}
