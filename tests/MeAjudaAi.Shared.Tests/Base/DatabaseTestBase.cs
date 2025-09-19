using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Respawn;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Shared.Tests.Base;

/// <summary>
/// Classe base para testes de integração que requerem um banco de dados PostgreSQL.
/// Utiliza TestContainers para criar uma instância real do PostgreSQL.
/// Utiliza Respawn para limpar o banco de dados entre os testes.
/// </summary>
public abstract class DatabaseTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private Respawner? _respawner;

    protected DatabaseTestBase()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17.5")
            .WithDatabase("meajudaai_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .Build();
    }

    /// <summary>
    /// String de conexão para o banco de dados de teste
    /// </summary>
    protected string ConnectionString => _postgresContainer.GetConnectionString();

    /// <summary>
    /// Cria um DbContextOptions para o tipo de DbContext especificado
    /// </summary>
    protected DbContextOptions<TContext> CreateDbContextOptions<TContext>() where TContext : DbContext
    {
        return new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(ConnectionString)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;
    }

    /// <summary>
    /// Reseta o banco de dados para um estado limpo.
    /// Chame este método na configuração do teste ou entre testes, se necessário.
    /// </summary>
    protected async Task ResetDatabaseAsync()
    {
        if (_respawner == null)
            throw new InvalidOperationException("Banco de dados não inicializado. Chame InitializeAsync primeiro.");

        using var connection = new Npgsql.NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// Executa SQL bruto no banco de dados de teste.
    /// Útil para configuração ou verificação em testes.
    /// </summary>
    protected async Task ExecuteSqlAsync(string sql)
    {
        using var connection = new Npgsql.NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Inicializa o container do banco de dados de teste
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        // Inicia o container PostgreSQL
        await _postgresContainer.StartAsync();

        // Aguarda um pouco para o PostgreSQL ficar pronto
        await Task.Delay(1000);

        // Executa scripts de inicialização do banco (com timeout)
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        try
        {
            await InitializeDatabaseAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Se timeout, continua sem inicialização customizada
            // As migrações do EF irão configurar o que for necessário
        }

        // Respawner será inicializado depois que as migrações forem aplicadas
    }

    /// <summary>
    /// Executa a inicialização do banco de dados (agora simplificada)
    /// EF Core migrations irão configurar tudo que for necessário
    /// </summary>
    private async Task InitializeDatabaseAsync(CancellationToken cancellationToken = default)
    {
        // Simplificado: EF Core migrations são suficientes para testes
        // Não precisamos mais de scripts SQL customizados
        await Task.CompletedTask;
    }

    /// <summary>
    /// Inicializa o Respawner após as migrações serem aplicadas
    /// </summary>
    public async Task InitializeRespawnerAsync()
    {
        if (_respawner != null) return; // Já inicializado

        using var connection = new Npgsql.NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        
        // Aguarda até que pelo menos uma tabela seja criada
        var maxAttempts = 10;
        var attempt = 0;
        
        while (attempt < maxAttempts)
        {
            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = @"
                SELECT COUNT(*) 
                FROM information_schema.tables 
                WHERE table_schema IN ('public', 'users')
                AND table_type = 'BASE TABLE'
                AND table_name != '__EFMigrationsHistory'";
            
            var tableCount = (long)(await checkCommand.ExecuteScalarAsync() ?? 0L);
            
            if (tableCount > 0)
            {
                break; // Tabelas encontradas, pode inicializar o Respawner
            }
            
            attempt++;
            await Task.Delay(500); // Aguarda 500ms antes de tentar novamente
        }
        
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public", "users"], // Apenas schema de users por enquanto
            TablesToIgnore = ["__EFMigrationsHistory"],
            WithReseed = true
        });
    }

    /// <summary>
    /// Limpa o container do banco de dados de teste
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}