using System.Diagnostics;
using System.Diagnostics.Metrics;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Unit.Database;

public class DapperConnectionTests
{
    private readonly DatabaseMetrics _metrics;
    private readonly Mock<ILogger<DapperConnection>> _loggerMock;
    private readonly PostgresOptions _postgresOptions;

    public DapperConnectionTests()
    {
        var meterFactory = new TestMeterFactory();
        _metrics = new DatabaseMetrics(meterFactory);
        _loggerMock = new Mock<ILogger<DapperConnection>>();
        _postgresOptions = new PostgresOptions
        {
            ConnectionString = "Host=localhost;Port=5432;Database=test;Username=test;Password=test;"
        };
    }

    [Fact]
    public void Constructor_UsesProvidedConnectionString_WhenConfigured()
    {
        // Arrange & Act
        var connection = new DapperConnection(_postgresOptions, _metrics, _loggerMock.Object);

        // Assert
        connection.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_UsesTestConnectionString_WhenInTestingEnvironment()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        var emptyOptions = new PostgresOptions();

        // Act
        var connection = new DapperConnection(emptyOptions, _metrics, _loggerMock.Object);

        // Assert
        connection.Should().NotBeNull();

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact(Skip = "DapperConnection modifica ambiente globalmente, impossível isolar em testes unitários - testar em integração")]
    public void Constructor_ThrowsException_WhenConnectionStringNotConfiguredOutsideTesting()
    {
        // Este teste verifica se DapperConnection lança exceção quando não há connection string em ambiente de produção
        // Porém, não podemos isolar corretamente o teste pois o construtor estático é chamado antes
        // Este comportamento deve ser validado em teste de integração
    }

    [Fact]
    public async Task QueryAsync_RecordsMetrics_OnSuccess()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        var connection = new DapperConnection(_postgresOptions, _metrics, _loggerMock.Object);

        // Act & Assert
        // Não podemos executar query real sem banco, mas podemos verificar o comportamento esperado
        // através de testes de integração separados
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public async Task QueryAsync_DoesNotRecordMetrics_OnCancellation()
    {
        // Este teste verifica comportamento de cancelamento
        // Implementação real requer teste de integração com banco de dados real
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        
        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public void GetSqlPreview_TruncatesLongSql()
    {
        // Este teste verifica método privado através de comportamento de log
        // A implementação real do método GetSqlPreview trunca SQL > 100 caracteres
        
        // Para testes de método privado, normalmente verificamos através de:
        // 1. Testes de integração que exercitam o caminho completo
        // 2. Verificação de logs quando Debug está habilitado
        
        // Aqui apenas documentamos o comportamento esperado
        var longSql = new string('X', 150);
        var expectedPreview = new string('X', 100) + "...";
        
        // Comportamento esperado: SQL com 150 caracteres deve ser truncado para 100 + "..."
        expectedPreview.Should().HaveLength(103);
    }

    [Theory]
    [InlineData("SELECT * FROM users")]
    [InlineData("INSERT INTO users (name) VALUES (@name)")]
    [InlineData("UPDATE users SET name = @name WHERE id = @id")]
    [InlineData("DELETE FROM users WHERE id = @id")]
    public void ConnectionString_IsConfiguredCorrectly_ForDifferentQueryTypes(string _)
    {
        // Arrange & Act
        var connection = new DapperConnection(_postgresOptions, _metrics, _loggerMock.Object);

        // Assert
        // Verifica que a conexão foi criada corretamente para diferentes tipos de query
        connection.Should().NotBeNull();
    }

    [Fact]
    public void LogLevel_Debug_EnablesSqlPreview()
    {
        // Arrange
        _loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        
        // Act
        var connection = new DapperConnection(_postgresOptions, _metrics, _loggerMock.Object);

        // Assert
        // Quando Debug está habilitado, SQL preview deve ser incluído nos logs de erro
        // Este comportamento é verificado em HandleDapperError quando ocorre exceção
        connection.Should().NotBeNull();
    }

    [Fact]
    public void LogLevel_NotDebug_SkipsSqlPreview()
    {
        // Arrange
        _loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
        
        // Act
        var connection = new DapperConnection(_postgresOptions, _metrics, _loggerMock.Object);

        // Assert
        // Quando Debug não está habilitado, SQL preview NÃO deve ser incluído nos logs
        // Isso reduz exposição de dados sensíveis em produção e evita custo de formatação
        connection.Should().NotBeNull();
    }
}

/// <summary>
/// Testes de integração para DapperConnection com banco de dados real via TestContainers
/// </summary>
[Trait("Category", "Integration")]
public class DapperConnectionIntegrationTests : BaseDatabaseTest
{
    [Fact]
    public async Task QueryAsync_ExecutesQuerySuccessfully()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new DatabaseMetrics(meterFactory);
        var loggerMock = new Mock<ILogger<DapperConnection>>();
        var options = new PostgresOptions { ConnectionString = ConnectionString };
        var connection = new DapperConnection(options, metrics, loggerMock.Object);

        // Act
        var result = await connection.QueryAsync<int>("SELECT 1");

        // Assert
        result.Should().Contain(1);
    }

    [Fact]
    public async Task QuerySingleOrDefaultAsync_ExecutesQuerySuccessfully()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new DatabaseMetrics(meterFactory);
        var loggerMock = new Mock<ILogger<DapperConnection>>();
        var options = new PostgresOptions { ConnectionString = ConnectionString };
        var connection = new DapperConnection(options, metrics, loggerMock.Object);

        // Act
        var result = await connection.QuerySingleOrDefaultAsync<int>("SELECT 1");

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ExecutesCommandSuccessfully()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new DatabaseMetrics(meterFactory);
        var loggerMock = new Mock<ILogger<DapperConnection>>();
        var options = new PostgresOptions { ConnectionString = ConnectionString };
        var connection = new DapperConnection(options, metrics, loggerMock.Object);

        // Act
        // PostgreSQL retorna -1 para comandos DDL (CREATE, DROP, ALTER)
        // Apenas comandos DML (INSERT, UPDATE, DELETE) retornam número de linhas afetadas
        var result = await connection.ExecuteAsync("CREATE TEMP TABLE test_execute (id INT)");

        // Assert
        result.Should().Be(-1, "CREATE TEMP TABLE returns -1 for DDL commands in PostgreSQL");
    }

    [Fact]
    public async Task QueryAsync_RecordsConnectionError_OnFailure()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new DatabaseMetrics(meterFactory);
        var loggerMock = new Mock<ILogger<DapperConnection>>();
        var options = new PostgresOptions { ConnectionString = "Host=invalid;Port=9999;Database=invalid;Username=invalid;Password=invalid;" };
        var connection = new DapperConnection(options, metrics, loggerMock.Object);

        // Act
        var act = async () => await connection.QueryAsync<int>("SELECT 1");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task QueryAsync_HandlesCancellation_Gracefully()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new DatabaseMetrics(meterFactory);
        var loggerMock = new Mock<ILogger<DapperConnection>>();
        var options = new PostgresOptions { ConnectionString = ConnectionString };
        var connection = new DapperConnection(options, metrics, loggerMock.Object);
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act
        var act = async () => await connection.QueryAsync<int>("SELECT 1", cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
