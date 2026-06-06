using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MeAjudaAi.Integration.Tests;

/// <summary>
/// 🧪 TESTE ISOLADO PARA VALIDAR A CORREÇÃO DE CONNECTION STRING
/// 
/// Este teste valida especificamente nossa melhoria na configuração
/// de connection string sem depender de endpoints HTTP.
/// </summary>
public class ConnectionStringValidationTests(ITestOutputHelper testOutput) : BaseApiTest
{
    /// <summary>
    /// ✅ Teste para validar que a estratégia de fallback de connection string funciona
    /// </summary>
    [Fact]
    public async Task ConnectionString_Fallback_Strategy_Should_Work()
    {
        // Arrange: Obtém o contexto do banco de dados
        var dbContext = Services.GetRequiredService<ProvidersDbContext>();

        // Act: Tenta conectar ao banco de dados
        var canConnect = await dbContext.Database.CanConnectAsync();

        // Assert: Verifica se a conexão foi estabelecida com sucesso
        testOutput.WriteLine($"✅ Database connection established: {canConnect}");
        canConnect.Should().BeTrue("Database connection should work with our fallback strategy");
    }

    /// <summary>
    /// ✅ Teste para validar que as tabelas do Provider estão criadas
    /// </summary>
    [Fact]
    public async Task ProvidersDatabase_Tables_Should_Exist()
    {
        // Arrange
        var dbContext = Services.GetRequiredService<ProvidersDbContext>();

        // Act: Verifica se as tabelas existem tentando uma query simples
        var tableExists = true;
        try
        {
            var count = await dbContext.Providers.CountAsync();
            testOutput.WriteLine($"✅ Providers table exists, current count: {count}");
        }
        catch (Exception ex)
        {
            tableExists = false;
            testOutput.WriteLine($"❌ Error accessing Providers table: {ex.Message}");
        }

        // Assert
        tableExists.Should().BeTrue("Providers table should exist and be accessible");
    }

    /// <summary>
    /// ✅ Teste para validar que os contextos de banco estão configurados corretamente
    /// </summary>
    [Fact]
    public async Task Database_Contexts_Should_Be_Configured()
    {
        // Arrange
        var usersContext = Services.GetRequiredService<UsersDbContext>();
        var providersContext = Services.GetRequiredService<ProvidersDbContext>();

        // Act: Verifica se os contextos conseguem conectar
        var usersCanConnect = await usersContext.Database.CanConnectAsync();
        var providersCanConnect = await providersContext.Database.CanConnectAsync();

        // Log connection status
        testOutput.WriteLine($"Users context can connect: {usersCanConnect}");
        testOutput.WriteLine($"Providers context can connect: {providersCanConnect}");

        // Log connection strings (for debugging)
        var usersConnectionString = usersContext.Database.GetConnectionString();
        var providersConnectionString = providersContext.Database.GetConnectionString();

        testOutput.WriteLine($"Users connection configured: {!string.IsNullOrEmpty(usersConnectionString)}");
        testOutput.WriteLine($"Providers connection configured: {!string.IsNullOrEmpty(providersConnectionString)}");

        // Assert: Both contexts should be able to connect
        usersCanConnect.Should().BeTrue("Users database context should be properly configured");
        providersCanConnect.Should().BeTrue("Providers database context should be properly configured");
    }
}
