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
using Npgsql;

namespace MeAjudaAi.Modules.Providers.Tests.Integration;

/// <summary>
/// Classe base para testes de integração específicos do módulo Providers.
/// Otimizada para usar um container compartilhado (Singleton) com bancos de dados isolados.
/// </summary>
[Trait("Category", "Integration")]
public abstract class ProvidersIntegrationTestBase : IAsyncLifetime
{
    // Container estático compartilhado entre TODOS os testes desta classe
    private static PostgreSqlContainer? _sharedContainer;
    private static readonly SemaphoreSlim _sharedContainerLock = new(1, 1);
    
    // Cache de nomes de bancos para garantir reutilização por TYPE da classe de teste
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, string> _databaseNames = new();
    // Use Lazy<Task> to ensure only one creation task runs per database name, even with concurrent tests
    // Usar Lazy<Task> para garantir que apenas uma tarefa de criação seja executada por nome de banco de dados, mesmo com testes simultâneos
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Lazy<Task>> _createdDatabases = new();

    private ServiceProvider? _serviceProvider;
    private ProvidersDbContext? _dbContext;
    private readonly string _testClassId;
    private string? _connectionString;

    protected ProvidersIntegrationTestBase()
    {
        // Garante que a mesma classe de teste use sempre o mesmo nome base, 
        // mas classes diferentes (mesmo rodando em paralelo) tenham nomes diferentes.
        _testClassId = _databaseNames.GetOrAdd(GetType(), t => $"{t.Name}_{Guid.NewGuid():N}");
    }

    /// <summary>
    /// Configurações padrão para testes do módulo Providers.
    /// InitializeDatabaseAsync (chamado via IAsyncLifetime) executa TRUNCATE em todas as tabelas antes de cada método de teste,
    /// garantindo um estado limpo. CleanupDatabase() é mantido como no-op para compatibilidade.
    /// </summary>
    protected TestInfrastructureOptions GetTestOptions()
    {
        var dbName = $"providers_test_{_testClassId}";
        if (dbName.Length > 63)
        {
            // keep uniqueness via GUID suffix (last 32 chars) + prefix to identify it's a test
            // Manter unicidade via sufixo GUID (últimos 32 caracteres) + prefixo para identificar que é um teste
            // pt_ + 15 chars prefix + _ + 32 chars GUID = 51 chars < 63 limit
            var prefix = GetType().Name.Length > 15 ? GetType().Name[..15] : GetType().Name;
            dbName = $"pt_{prefix}_{_testClassId[^32..]}";
        }

        var options = new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                DatabaseName = dbName,
                Username = "postgres",
                Password = "test123",
                Schema = "providers",
                // Força o uso do container compartilhado sobrescrevendo a connection string posteriormente
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

        // Se já temos connection string (container já subiu), usamos ela
        if (_connectionString != null)
        {
            options.Database.ConnectionString = _connectionString;
        }

        return options;
    }

    /// <summary>
    /// Inicialização executada antes de cada método de teste
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        // 1. Garantir que o container compartilhado está rodando
        await EnsureSharedContainerAsync();

        // 2. Criar um banco de dados lógico ÚNICO para este teste dentro do container compartilhado
        var options = GetTestOptions();
        var databaseName = options.Database.DatabaseName;
        
        // Get or add a Lazy<Task> to ensure we only create the database once
        // Obter ou adicionar um Lazy<Task> para garantir que criamos o banco de dados apenas uma vez
        var creationTask = _createdDatabases.GetOrAdd(databaseName, 
            _ => new Lazy<Task>(() => CreateLogicalDatabaseAsync(databaseName)));
            
        // Await the task (wrapper for the actual creation logic)
        // Aguardar a tarefa (wrapper para a lógica de criação real)
        await creationTask.Value;
        
        // Atualiza connection string para apontar para o novo banco
        var builder = new NpgsqlConnectionStringBuilder(_sharedContainer!.GetConnectionString())
        {
            Database = databaseName
        };
        _connectionString = builder.ToString();
        
        // Atualiza options com a nova connection string
        options.Database.ConnectionString = _connectionString;

        // 3. Configurar serviços
        var services = new ServiceCollection();

        // Configurar logging otimizado
        services.AddLogging(builder =>
        {
            builder.ConfigureTestLogging();
        });

        // Configurar serviços específicos do módulo
        ConfigureModuleServices(services, options);

        _serviceProvider = services.BuildServiceProvider();

        // 4. Inicializar schema do banco (EnsureCreated)
        await InitializeDatabaseAsync();
    }

    private static async Task EnsureSharedContainerAsync()
    {
        if (_sharedContainer != null) return;

        await _sharedContainerLock.WaitAsync();
        try
        {
            if (_sharedContainer != null) return;

            // Configuração global para Npgsql
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var container = new PostgreSqlBuilder("postgis/postgis:16-3.4")
                .WithDatabase("postgres") // Banco padrão para conexões administrativas
                .WithUsername("postgres")
                .WithPassword("test123")
                .WithCleanUp(true)
                .Build();

            await container.StartAsync();

            _sharedContainer = container;
        }
        finally
        {
            _sharedContainerLock.Release();
        }
    }

    private static async Task CreateLogicalDatabaseAsync(string databaseName)
    {
        // Conecta no banco 'postgres' padrão para criar o novo banco
        var adminConnectionString = _sharedContainer!.GetConnectionString();
        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();

        // Cria o banco de dados
        await using var command = connection.CreateCommand();
        // Sanitize database name to prevent injection/errors with weird chars
        // Higienizar nome do banco de dados para evitar injeção/erros com caracteres estranhos
        var safeName = databaseName.Replace("\"", "\"\"");
        command.CommandText = $"CREATE DATABASE \"{safeName}\"";
        
        try 
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (PostgresException ex) when (ex.SqlState == "42P04") // Duplicate database
        {
            // Ignore if already exists (should be handled by Lazy, but good for safety)
            // Ignorar se já existe (deve ser tratado pelo Lazy, mas bom para segurança)
        }
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

        // Criar esquema
        await dbContext.Database.EnsureCreatedAsync();

        // Verificar isolamento (não deve ter providers ainda)
        // Como estamos reusando o banco por classe de teste, precisamos limpar os dados entre os testes
        var connection = dbContext.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await connection.OpenAsync();

        try 
        {
            // Script para truncar todas as tabelas do schema 'providers'
            await dbContext.Database.ExecuteSqlRawAsync(@"
                DO $$ DECLARE
                    r RECORD;
                BEGIN
                    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'providers') LOOP
                        EXECUTE 'TRUNCATE TABLE ' || quote_ident('providers') || '.' || quote_ident(r.tablename) || ' CASCADE';
                    END LOOP;
                END $$;
            ");
        }
        finally
        {
            if (!wasOpen) await connection.CloseAsync();
        }

        // Sanity check
        // Verificação de segurança
        var count = await dbContext.Providers.CountAsync();
        if (count > 0)
        {
            throw new InvalidOperationException($"Database isolation failed: found {count} existing providers in new database");
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
        var dbContext = DbContext;
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
    /// Limpeza executada após cada método de teste
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
            _serviceProvider = null;
        }
        _dbContext = null;

        // NÃO descartamos o _sharedContainer aqui. Ele é estático e deve viver até o fim do processo.
        // Ryuk (Testcontainers) cuidará da limpeza ao final do processo.
    }

    /// <summary>
    /// Limpa dados das tabelas para isolamento entre testes.
    /// Como cada classe de teste usa um banco de dados isolado, este método é mantido para compatibilidade
    /// com testes que esperam poder limpar o estado explicitamente, mas o isolamento principal é via DB único.
    /// </summary>
    protected Task CleanupDatabase()
    {
        // No-op: Isolamento é garantido por banco de dados único por classe de teste.
        // Se um teste específico precisar de limpeza intra-classe, pode implementar delete manual.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Força limpeza do banco de dados.
    /// Mantido para compatibilidade.
    /// </summary>
    protected Task ForceCleanDatabase()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Acesso direto ao contexto de banco de dados do módulo Providers
    /// </summary>
    protected ProvidersDbContext DbContext => _dbContext ??= GetService<ProvidersDbContext>();

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
