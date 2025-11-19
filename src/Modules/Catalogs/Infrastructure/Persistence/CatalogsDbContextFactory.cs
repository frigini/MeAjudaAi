using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeAjudaAi.Modules.Catalogs.Infrastructure.Persistence;

/// <summary>
/// Fábrica em tempo de design para criar CatalogsDbContext durante as migrações do EF Core.
/// Isso permite que as migrações sejam criadas sem executar a aplicação completa.
/// </summary>
/// <remarks>
/// <para><strong>Configuração Requerida:</strong></para>
/// <para>
/// Esta fábrica requer a variável de ambiente <c>CATALOGS_DB_CONNECTION</c> contendo
/// a string de conexão Npgsql para operações de design-time do EF Core.
/// </para>
/// <para><strong>Como Configurar:</strong></para>
/// <list type="bullet">
/// <item><description>PowerShell: <c>$env:CATALOGS_DB_CONNECTION="Host=localhost;Port=5432;Database=meajudaai;Username=postgres;Password=yourpass"</c></description></item>
/// <item><description>Bash/Linux: <c>export CATALOGS_DB_CONNECTION="Host=localhost;Port=5432;Database=meajudaai;Username=postgres;Password=yourpass"</c></description></item>
/// <item><description>IDE: Configurar nas variáveis de ambiente do projeto/run configuration</description></item>
/// <item><description>Arquivo .env na raiz do projeto (se suportado pela sua ferramenta de build)</description></item>
/// </list>
/// <para>
/// <strong>Nota:</strong> Esta configuração é usada apenas para geração de migrações (<c>dotnet ef migrations add</c>),
/// não para execução da aplicação em runtime.
/// </para>
/// </remarks>
public sealed class CatalogsDbContextFactory : IDesignTimeDbContextFactory<CatalogsDbContext>
{
    public CatalogsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogsDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("CATALOGS_DB_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "CATALOGS_DB_CONNECTION environment variable is not set. " +
                "This is required for EF Core design-time operations (migrations). " +
                "Set it in your shell (e.g., $env:CATALOGS_DB_CONNECTION='Host=localhost;...'), " +
                "IDE run configuration, or .env file.");
        }

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "catalogs");
                npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

        return new CatalogsDbContext(optionsBuilder.Options);
    }
}
