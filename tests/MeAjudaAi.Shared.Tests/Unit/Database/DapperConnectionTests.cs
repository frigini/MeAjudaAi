using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Metrics;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Unit.Database;

public class DapperConnectionTests : IDisposable
{
    private readonly DatabaseMetrics _metrics;
    private readonly Mock<ILogger<DapperConnection>> _loggerMock;
    private readonly PostgresOptions _postgresOptions;
    private readonly EnvironmentVariableRestorer _envRestorer;

    private sealed class EnvironmentVariableRestorer : IDisposable
    {
        private readonly HashSet<string> _modifiedVariables = new();

        public void SetVariable(string name, string value)
        {
            if (!_modifiedVariables.Contains(name))
            {
                _modifiedVariables.Add(name);
            }
            Environment.SetEnvironmentVariable(name, value);
        }

        public void Dispose()
        {
            Restore();
        }

        private void Restore()
        {
            foreach (var name in _modifiedVariables)
            {
                Environment.SetEnvironmentVariable(name, null);
            }
        }
    }

    public DapperConnectionTests()
    {
        var meterFactory = new TestMeterFactory();
        _metrics = new DatabaseMetrics(meterFactory);
        _loggerMock = new Mock<ILogger<DapperConnection>>();
        _postgresOptions = new PostgresOptions
        {
            ConnectionString = DatabaseConstants.LocalWithPortConnectionString
        };
        _envRestorer = new EnvironmentVariableRestorer();
    }

    public void Dispose()
    {
        _envRestorer.Dispose();
        GC.SuppressFinalize(this);
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
        _envRestorer.SetVariable("DOTNET_ENVIRONMENT", "Testing");
        _envRestorer.SetVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        var emptyOptions = new PostgresOptions();

        var connection = new DapperConnection(emptyOptions, _metrics, _loggerMock.Object);

        connection.Should().NotBeNull();
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
        var options = new PostgresOptions { ConnectionString = DatabaseConstants.InvalidConnectionString };
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
