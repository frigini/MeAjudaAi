using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Tests;
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
        Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "true");

        _databaseFixture = new SimpleDatabaseFixture();
        await _databaseFixture.InitializeAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Substitui banco de dados por container de teste - Remove todos os serviços relacionados ao DbContext
                    RemoveDbContextRegistrations<UsersDbContext>(services);
                    RemoveDbContextRegistrations<ProvidersDbContext>(services);
                    RemoveDbContextRegistrations<DocumentsDbContext>(services);

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

                    services.AddDbContext<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>(options =>
                    {
                        options.UseNpgsql(_databaseFixture.ConnectionString, npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Documents.Infrastructure");
                            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "documents");
                        });
                        options.EnableSensitiveDataLogging();
                        options.ConfigureWarnings(warnings =>
                            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });

                    // Adiciona mocks de serviços para testes
                    services.AddDocumentsTestServices();

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
        // Nota: Todos os módulos usam setup baseado em migrações para consistência com produção
        using var scope = _factory.Services.CreateScope();
        var usersContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var providersContext = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        var documentsContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<ApiTestBase>>();

        // Aplica migrações exatamente como nos testes E2E
        await ApplyMigrationsAsync(usersContext, providersContext, documentsContext, logger);
    }

    private static async Task ApplyMigrationsAsync(
        UsersDbContext usersContext,
        ProvidersDbContext providersContext,
        DocumentsDbContext documentsContext,
        ILogger? logger)
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

        // Aplica migrações em todos os módulos
        await ApplyMigrationForContextAsync(usersContext, "Users", logger, "UsersDbContext primeiro (cria database e schema users)");
        await ApplyMigrationForContextAsync(providersContext, "Providers", logger, "ProvidersDbContext (banco já existe, só precisa do schema providers)");
        await ApplyMigrationForContextAsync(documentsContext, "Documents", logger, "DocumentsDbContext (banco já existe, só precisa do schema documents)");

        // Verifica se as tabelas existem
        await VerifyContextAsync(usersContext, "Users", () => usersContext.Users.CountAsync(), logger);
        await VerifyContextAsync(providersContext, "Providers", () => providersContext.Providers.CountAsync(), logger);
        await VerifyContextAsync(documentsContext, "Documents", () => documentsContext.Documents.CountAsync(), logger);
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        _factory?.Dispose();
        if (_databaseFixture != null)
            await _databaseFixture.DisposeAsync();
    }

    /// <summary>
    /// Remove DbContextOptions e DbContext registrations do DI container.
    /// </summary>
    private static void RemoveDbContextRegistrations<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var optionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (optionsDescriptor != null)
            services.Remove(optionsDescriptor);

        var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TContext));
        if (contextDescriptor != null)
            services.Remove(contextDescriptor);
    }

    /// <summary>
    /// Aplica migrações para um DbContext específico com tratamento de erros padronizado.
    /// </summary>
    private static async Task ApplyMigrationForContextAsync<TContext>(
        TContext context,
        string moduleName,
        ILogger? logger,
        string? description = null) where TContext : DbContext
    {
        try
        {
            var message = description != null
                ? $"🔄 Aplicando migrações do módulo {moduleName} ({description})..."
                : $"🔄 Aplicando migrações do módulo {moduleName}...";
            logger?.LogInformation(message);

            await context.Database.MigrateAsync();
            logger?.LogInformation("✅ Migrações do banco {Module} completadas com sucesso", moduleName);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌ Falha ao aplicar migrações do {Module}: {Message}", moduleName, ex.Message);
            throw new InvalidOperationException($"Não foi possível aplicar migrações do banco {moduleName}", ex);
        }
    }

    /// <summary>
    /// Verifica se um DbContext está funcionando corretamente executando uma query de contagem.
    /// </summary>
    private static async Task VerifyContextAsync<TContext>(
        TContext context,
        string moduleName,
        Func<Task<int>> countQuery,
        ILogger? logger) where TContext : DbContext
    {
        try
        {
            var count = await countQuery();
            logger?.LogInformation("Verificação do banco {Module} bem-sucedida - Contagem: {Count}", moduleName, count);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Verificação do banco {Module} falhou", moduleName);
            throw new InvalidOperationException($"Banco {moduleName} não foi inicializado corretamente", ex);
        }
    }
}
