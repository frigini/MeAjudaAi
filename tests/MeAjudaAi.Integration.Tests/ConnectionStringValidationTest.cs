using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace MeAjudaAi.Integration.Tests;

/// <summary>
/// 🧪 TESTE ISOLADO PARA VALIDAR A CORREÇÃO DE CONNECTION STRING
/// 
/// Este teste valida especificamente nossa melhoria na configuração
/// de connection string sem depender de endpoints HTTP.
/// </summary>
public class ConnectionStringValidationTest(ITestOutputHelper testOutput) : ApiTestBase
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
    /// ✅ Teste para validar a configuração de connection string foi carregada corretamente
    /// </summary>
    [Fact]
    public void ConnectionString_Configuration_Should_Be_Valid()
    {
        // Arrange
        var configuration = Services.GetRequiredService<IConfiguration>();
        
        // Act: Verifica todas as possíveis connection strings na ordem de fallback
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");
        var providersConnection = configuration.GetConnectionString("Providers");
        var meAjudaAiConnection = configuration.GetConnectionString("meajudaai-db");
        
        // Assert: Pelo menos uma connection string deve estar disponível
        var hasValidConnection = !string.IsNullOrEmpty(defaultConnection) ||
                               !string.IsNullOrEmpty(providersConnection) ||
                               !string.IsNullOrEmpty(meAjudaAiConnection);
        
        testOutput.WriteLine($"DefaultConnection: {!string.IsNullOrEmpty(defaultConnection)}");
        testOutput.WriteLine($"Providers: {!string.IsNullOrEmpty(providersConnection)}");
        testOutput.WriteLine($"meajudaai-db: {!string.IsNullOrEmpty(meAjudaAiConnection)}");
        
        hasValidConnection.Should().BeTrue("At least one connection string should be configured");
    }
}