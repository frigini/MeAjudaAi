using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Classe base para f�bricas de DbContext em tempo de design em todos os m�dulos
/// Detecta automaticamente o nome do m�dulo a partir do namespace
/// </summary>
/// <typeparam name="TContext">O tipo do DbContext</typeparam>
public abstract class BaseDesignTimeDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    /// <summary>
    /// Obt�m o nome do m�dulo automaticamente a partir do namespace da classe derivada
    /// Padr�o de namespace esperado: MeAjudaAi.Modules.{ModuleName}.Infrastructure.Persistence
    /// </summary>
    protected virtual string GetModuleName()
    {
        var derivedType = GetType();
        var namespaceParts = derivedType.Namespace?.Split('.') ?? Array.Empty<string>();
        
        // Procura pelo padr�o: MeAjudaAi.Modules.{ModuleName}.Infrastructure
        for (int i = 0; i < namespaceParts.Length - 1; i++)
        {
            if (namespaceParts[i] == "MeAjudaAi" && 
                i + 2 < namespaceParts.Length && 
                namespaceParts[i + 1] == "Modules")
            {
                return namespaceParts[i + 2]; // Retorna o nome do m�dulo
            }
        }
        
        // Alternativa: extrai do nome da classe se seguir o padr�o {ModuleName}DbContextFactory
        var className = derivedType.Name;
        if (className.EndsWith("DbContextFactory"))
        {
            return className.Substring(0, className.Length - "DbContextFactory".Length);
        }
        
        throw new InvalidOperationException(
            $"N�o foi poss�vel determinar o nome do m�dulo a partir do namespace '{derivedType.Namespace}' ou do nome da classe '{className}'. " +
            "Padr�o de namespace esperado: 'MeAjudaAi.Modules.{ModuleName}.Infrastructure.Persistence' " +
            "ou padr�o de nome de classe: '{ModuleName}DbContextFactory'");
    }
    
    /// <summary>
    /// Obt�m a string de conex�o para opera��es em tempo de design
    /// Pode ser sobrescrito para l�gica personalizada
    /// </summary>
    protected virtual string GetDesignTimeConnectionString()
    {
        // Tenta obter da configura��o primeiro
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }
        
        // Alternativa para conex�o local padr�o de desenvolvimento
        return GetDefaultConnectionString();
    }
    
    /// <summary>
    /// Obt�m o nome do assembly de migrations com base no nome do m�dulo
    /// </summary>
    protected virtual string GetMigrationsAssembly()
    {
        return $"MeAjudaAi.Modules.{GetModuleName()}.Infrastructure";
    }
    
    /// <summary>
    /// Obt�m o nome do schema da tabela de hist�rico de migrations com base no nome do m�dulo
    /// </summary>
    protected virtual string GetMigrationsHistorySchema()
    {
        return GetModuleName().ToLowerInvariant();
    }
    
    /// <summary>
    /// Obt�m a string de conex�o padr�o para desenvolvimento local
    /// </summary>
    protected virtual string GetDefaultConnectionString()
    {
        var moduleName = GetModuleName().ToLowerInvariant();
        return $"Host=localhost;Database=meajudaai_dev;Username=postgres;Password=dev123;SearchPath={moduleName},public";
    }
    
    /// <summary>
    /// Constr�i a configura��o a partir dos arquivos appsettings
    /// </summary>
    protected virtual IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables();
        
        return builder.Build();
    }
    
    /// <summary>
    /// Configura op��es adicionais para o DbContext
    /// </summary>
    /// <param name="optionsBuilder">O builder de op��es</param>
    protected virtual void ConfigureAdditionalOptions(DbContextOptionsBuilder<TContext> optionsBuilder)
    {
        // Sobrescreva em classes derivadas se necess�rio
    }

    /// <summary>
    /// Cria a inst�ncia do DbContext para opera��es em tempo de design
    /// </summary>
    /// <param name="args">Argumentos de linha de comando</param>
    /// <returns>Inst�ncia configurada do DbContext</returns>
    public TContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        
        // Configura PostgreSQL com op��es de migrations
        optionsBuilder.UseNpgsql(GetDesignTimeConnectionString(), options =>
        {
            options.MigrationsAssembly(GetMigrationsAssembly());
            options.MigrationsHistoryTable("__EFMigrationsHistory", GetMigrationsHistorySchema());
        });
        
        // Permite que classes derivadas configurem op��es adicionais
        ConfigureAdditionalOptions(optionsBuilder);

        return CreateDbContextInstance(optionsBuilder.Options);
    }
    
    /// <summary>
    /// Cria a inst�ncia real do DbContext
    /// Sobrescreva este m�todo para l�gica personalizada de construtor
    /// </summary>
    /// <param name="options">As op��es configuradas</param>
    /// <returns>Inst�ncia do DbContext</returns>
    protected abstract TContext CreateDbContextInstance(DbContextOptions<TContext> options);
}