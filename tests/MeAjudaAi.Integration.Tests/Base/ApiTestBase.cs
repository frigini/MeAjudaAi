using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.Auth;
using MeAjudaAi.Shared.Tests.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Classe base unificada para testes de integração com suporte a autenticação baseada em instância.
/// Elimina condições de corrida e instabilidade causadas por estado estático.
/// Cria containers individuais para máxima compatibilidade com CI.
/// </summary>
public abstract class ApiTestBase : IAsyncLifetime
{
    private SimpleDatabaseFixture? _databaseFixture;
    private WebApplicationFactory<Program>? _factory;

    protected HttpClient Client { get; private set; } = null!;
    protected IServiceProvider Services => _factory!.Services;
    protected ITestAuthenticationConfiguration AuthConfig { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // Define variáveis de ambiente para testes
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");

        _databaseFixture = new SimpleDatabaseFixture();
        await _databaseFixture.InitializeAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Substitui banco de dados por container de teste - Remove todos os serviços relacionados ao DbContext
                    var usersDbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<UsersDbContext>));
                    if (usersDbContextDescriptor != null)
                        services.Remove(usersDbContextDescriptor);

                    var providersDbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ProvidersDbContext>));
                    if (providersDbContextDescriptor != null)
                        services.Remove(providersDbContextDescriptor);

                    // Remove também os serviços DbContext se existirem
                    var usersDbContextService = services.SingleOrDefault(d => d.ServiceType == typeof(UsersDbContext));
                    if (usersDbContextService != null)
                        services.Remove(usersDbContextService);

                    var providersDbContextService = services.SingleOrDefault(d => d.ServiceType == typeof(ProvidersDbContext));
                    if (providersDbContextService != null)
                        services.Remove(providersDbContextService);

                    // Adiciona contextos de banco de dados para testes
                    services.AddDbContext<UsersDbContext>(options =>
                    {
                        options.UseNpgsql(_databaseFixture.ConnectionString, npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Users.Infrastructure");
                            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "users");
                        });
                        options.EnableSensitiveDataLogging();
                        options.ConfigureWarnings(warnings =>
                            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });

                    services.AddDbContext<ProvidersDbContext>(options =>
                    {
                        options.UseNpgsql(_databaseFixture.ConnectionString, npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Providers.Infrastructure");
                            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "providers");
                        });
                        options.EnableSensitiveDataLogging();
                        options.ConfigureWarnings(warnings =>
                            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });

                    // Adiciona autenticação de teste baseada em instância para evitar estado estático
                    services.RemoveRealAuthentication();
                    services.AddInstanceTestAuthentication();

                    // Remove ClaimsTransformation que causa travamentos nos testes
                    var claimsTransformationDescriptor = services.FirstOrDefault(d =>
                        d.ServiceType == typeof(IClaimsTransformation));
                    if (claimsTransformationDescriptor != null)
                        services.Remove(claimsTransformationDescriptor);
                });

                // Habilita logging detalhado para debug
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                    logging.AddFilter("Microsoft.AspNetCore", LogLevel.Debug);
                    logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Debug);
                    logging.AddFilter("MeAjudaAi", LogLevel.Debug);
                });
            });

        Client = _factory.CreateClient();

        // Obtém a configuração de autenticação da instância do container DI
        AuthConfig = _factory.Services.GetRequiredService<ITestAuthenticationConfiguration>();

        // Aplica migrações do banco de dados para testes
        // Nota: Ambos os módulos usam setup baseado em migrações para consistência com produção
        using var scope = _factory.Services.CreateScope();
        var usersContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var providersContext = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<ApiTestBase>>();

        // Aplica migrações exatamente como nos testes E2E
        await ApplyMigrationsAsync(usersContext, providersContext, logger);
    }

    private static async Task ApplyMigrationsAsync(UsersDbContext usersContext, ProvidersDbContext providersContext, ILogger? logger)
    {
        // Garante estado limpo do banco de dados (como nos testes E2E)
        try
        {
            await usersContext.Database.EnsureDeletedAsync();
            logger?.LogInformation("🧹 Banco de dados existente limpo");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌ Falha crítica ao limpar banco existente: {Message}", ex.Message);
            throw new InvalidOperationException("Não foi possível limpar o banco de dados antes dos testes", ex);
        }

        // Aplica migrações no UsersDbContext primeiro (cria database e schema users)
        try
        {
            logger?.LogInformation("🔄 Aplicando migrações do módulo Users...");
            await usersContext.Database.MigrateAsync();
            logger?.LogInformation("✅ Migrações do banco Users completadas com sucesso");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌ Falha ao aplicar migrações do Users: {Message}", ex.Message);
            throw new InvalidOperationException("Não foi possível aplicar migrações do banco Users", ex);
        }

        // Aplica migrações no ProvidersDbContext (banco já existe, só precisa do schema providers)
        try
        {
            logger?.LogInformation("🔄 Aplicando migrações do módulo Providers...");
            await providersContext.Database.MigrateAsync();
            logger?.LogInformation("✅ Migrações do banco Providers completadas com sucesso");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌ Falha ao aplicar migrações do Providers: {Message}", ex.Message);
            throw new InvalidOperationException("Não foi possível aplicar migrações do banco Providers", ex);
        }

        // Verifica se as tabelas existem
        try
        {
            var usersCount = await usersContext.Users.CountAsync();
            logger?.LogInformation("Verificação do banco Users bem-sucedida - Contagem: {UsersCount}", usersCount);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Verificação do banco Users falhou");
            throw new InvalidOperationException("Banco Users não foi inicializado corretamente", ex);
        }

        try
        {
            var providersCount = await providersContext.Providers.CountAsync();
            logger?.LogInformation("Verificação do banco Providers bem-sucedida - Contagem: {ProvidersCount}", providersCount);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Verificação do banco Providers falhou");
            throw new InvalidOperationException("Banco Providers não foi inicializado corretamente", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        _factory?.Dispose();
        if (_databaseFixture != null)
            await _databaseFixture.DisposeAsync();
    }

    private static async Task CreateProvidersTableManually(ProvidersDbContext context, ILogger? logger)
    {
        logger?.LogInformation("🔨 Iniciando criação manual das tabelas do Providers com estado limpo");

        // Primeiro, remove tabelas existentes para garantir estado limpo
        await context.Database.ExecuteSqlRawAsync(@"
            DROP TABLE IF EXISTS providers.qualification CASCADE;
            DROP TABLE IF EXISTS providers.document CASCADE;
            DROP TABLE IF EXISTS providers.providers CASCADE;
        ");

        // Cria a tabela principal providers com todas as colunas necessárias baseadas no modelo EF Core
        var createProvidersTable = @"
            CREATE TABLE IF NOT EXISTS providers.providers (
                id uuid PRIMARY KEY,
                user_id uuid NOT NULL,
                name varchar(100) NOT NULL,
                type varchar(20) NOT NULL,
                verification_status varchar(20) NOT NULL,
                is_deleted boolean NOT NULL DEFAULT false,
                deleted_at timestamp with time zone,
                created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                updated_at timestamp with time zone,
                legal_name varchar(200) NOT NULL,
                fantasy_name varchar(200),
                description varchar(1000),
                email varchar(255) NOT NULL,
                phone_number varchar(20),
                website varchar(255),
                street varchar(200) NOT NULL,
                number varchar(20) NOT NULL,
                complement varchar(100),
                neighborhood varchar(100) NOT NULL,
                city varchar(100) NOT NULL,
                state varchar(50) NOT NULL,
                zip_code varchar(20) NOT NULL,
                country varchar(50) NOT NULL
            );";

        // Cria a tabela de documentos (entidade owned)
        var createDocumentsTable = @"
            CREATE TABLE IF NOT EXISTS providers.document (
                provider_id uuid NOT NULL,
                id serial PRIMARY KEY,
                number varchar(50) NOT NULL,
                document_type varchar(20) NOT NULL,
                is_primary boolean NOT NULL DEFAULT false,
                FOREIGN KEY (provider_id) REFERENCES providers.providers(id) ON DELETE CASCADE
            );";

        // Cria a tabela de qualificações (entidade owned)
        var createQualificationsTable = @"
            CREATE TABLE IF NOT EXISTS providers.qualification (
                provider_id uuid NOT NULL,
                id serial PRIMARY KEY,
                name varchar(200) NOT NULL,
                description varchar(1000),
                issuing_organization varchar(200),
                issue_date timestamp with time zone,
                expiration_date timestamp with time zone,
                document_number varchar(50),
                FOREIGN KEY (provider_id) REFERENCES providers.providers(id) ON DELETE CASCADE
            );";

        var createIndices = @"
            CREATE UNIQUE INDEX IF NOT EXISTS ix_providers_user_id ON providers.providers (user_id);
            CREATE INDEX IF NOT EXISTS ix_providers_name ON providers.providers (name);
            CREATE INDEX IF NOT EXISTS ix_providers_type ON providers.providers (type);
            CREATE INDEX IF NOT EXISTS ix_providers_verification_status ON providers.providers (verification_status);
            CREATE INDEX IF NOT EXISTS ix_providers_is_deleted ON providers.providers (is_deleted);
            CREATE UNIQUE INDEX IF NOT EXISTS ix_document_provider_id_document_type ON providers.document (provider_id, document_type);";

        try
        {
            await context.Database.ExecuteSqlRawAsync(createProvidersTable);
            logger?.LogInformation("✅ Tabela providers criada manualmente");

            await context.Database.ExecuteSqlRawAsync(createDocumentsTable);
            logger?.LogInformation("✅ Tabela documents criada manualmente");

            await context.Database.ExecuteSqlRawAsync(createQualificationsTable);
            logger?.LogInformation("✅ Tabela qualifications criada manualmente");

            await context.Database.ExecuteSqlRawAsync(createIndices);
            logger?.LogInformation("✅ Índices criados manualmente");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌ Falha ao criar tabelas manualmente");
            throw;
        }
    }
}
