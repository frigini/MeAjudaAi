using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;

namespace MeAjudaAi.Integration.Tests.Providers;

/// <summary>
/// Testes para verificar se o ProvidersDbContext está funcionando corretamente
/// </summary>
public class ProvidersDbContextTests : ApiTestBase
{
    [Fact]
    public async Task CanConnectToDatabase_ShouldWork()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();

        // Act & Assert
        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task Database_ShouldHaveCorrectSchema()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();

        // Act
        var tables = await context.Database.SqlQueryRaw<string>(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'providers'"
        ).ToListAsync();

        // Assert
        tables.Should().Contain("providers");
        tables.Should().Contain("Document");
        tables.Should().Contain("Qualification");
    }

    [Fact]
    public async Task ProvidersTable_ShouldHaveCorrectStructure()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();

        // Act
        var columns = await context.Database.SqlQueryRaw<string>(
            @"SELECT column_name 
              FROM information_schema.columns 
              WHERE table_schema = 'providers' 
              AND table_name = 'providers'"
        ).ToListAsync();

        // Assert
        columns.Should().Contain("id");
        columns.Should().Contain("user_id");
        columns.Should().Contain("name");
        columns.Should().Contain("type");
        columns.Should().Contain("verification_status");
        columns.Should().Contain("legal_name");
        columns.Should().Contain("email");
        columns.Should().Contain("street");
        columns.Should().Contain("city");
        columns.Should().Contain("state");
        columns.Should().Contain("is_deleted");
        columns.Should().Contain("CreatedAt");
        columns.Should().Contain("UpdatedAt");
    }

    [Fact]
    public async Task MigrationsHistory_ShouldExistInCorrectSchema()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();

        // Act
        var historyExists = await context.Database.SqlQueryRaw<bool>(
            @"SELECT EXISTS (
                SELECT 1 FROM information_schema.tables 
                WHERE table_schema = 'providers' 
                AND table_name = '__EFMigrationsHistory'
              )"
        ).FirstAsync();

        // Assert
        historyExists.Should().BeTrue();
    }

    [Fact]
    public async Task ProvidersSchema_ShouldExist()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();

        // Act
        var schemaExists = await context.Database.SqlQueryRaw<bool>(
            @"SELECT EXISTS (
                SELECT 1 FROM information_schema.schemata 
                WHERE schema_name = 'providers'
              )"
        ).FirstAsync();

        // Assert
        schemaExists.Should().BeTrue();
    }
}