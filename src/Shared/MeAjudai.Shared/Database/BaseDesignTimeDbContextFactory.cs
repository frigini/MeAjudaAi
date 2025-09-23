using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Classe base para fábricas de DbContext em tempo de design em todos os módulos
/// Detecta automaticamente o nome do módulo a partir do namespace
/// </summary>
/// <typeparam name="TContext">O tipo do DbContext</typeparam>
public abstract class BaseDesignTimeDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    /// <summary>
    /// Obtém o nome do módulo automaticamente a partir do namespace da classe derivada
    /// Padrão de namespace esperado: MeAjudaAi.Modules.{ModuleName}.Infrastructure.Persistence
    /// </summary>
    protected virtual string GetModuleName()
    {
        var derivedType = GetType();
        var namespaceParts = derivedType.Namespace?.Split('.') ?? Array.Empty<string>();
        
        // Procura pelo padrão: MeAjudaAi.Modules.{ModuleName}.Infrastructure
        for (int i = 0; i < namespaceParts.Length - 1; i++)
        {
            if (namespaceParts[i] == "MeAjudaAi" && 
                i + 2 < namespaceParts.Length && 
                namespaceParts[i + 1] == "Modules")
            {
                return namespaceParts[i + 2]; // Retorna o nome do módulo
            }
        }
        
        // Alternativa: extrai do nome da classe se seguir o padrão {ModuleName}DbContextFactory
        var className = derivedType.Name;
        if (className.EndsWith("DbContextFactory"))
        {
            return className.Substring(0, className.Length - "DbContextFactory".Length);
        }
        
        throw new InvalidOperationException(
            $"Não foi possível determinar o nome do módulo a partir do namespace '{derivedType.Namespace}' ou do nome da classe '{className}'. " +
            "Padrão de namespace esperado: 'MeAjudaAi.Modules.{ModuleName}.Infrastructure.Persistence' " +
            "ou padrão de nome de classe: '{ModuleName}DbContextFactory'");
    }
    
    /// <summary>
    /// Obtém a string de conexão para operações em tempo de design
    /// Pode ser sobrescrito para lógica personalizada
    /// </summary>
    protected virtual string GetDesignTimeConnectionString()
    {
        // Tenta obter da configuração primeiro
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }
        
        // Alternativa para conexão local padrão de desenvolvimento
        return GetDefaultConnectionString();
    }
    
    /// <summary>
    /// Obtém o nome do assembly de migrations com base no nome do módulo
    /// </summary>
    protected virtual string GetMigrationsAssembly()
    {
        return $"MeAjudaAi.Modules.{GetModuleName()}.Infrastructure";
    }
    
    /// <summary>
    /// Obtém o nome do schema da tabela de histórico de migrations com base no nome do módulo
    /// </summary>
    protected virtual string GetMigrationsHistorySchema()
    {
        return GetModuleName().ToLowerInvariant();
    }
    
    /// <summary>
    /// Obtém a string de conexão padrão para desenvolvimento local
    /// </summary>
    protected virtual string GetDefaultConnectionString()
    {
        var moduleName = GetModuleName().ToLowerInvariant();
        return $"Host=localhost;Database=meajudaai_dev;Username=postgres;Password=dev123;SearchPath={moduleName},public";
    }
    
    /// <summary>
    /// Constrói a configuração a partir dos arquivos appsettings
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
    /// Configura opções adicionais para o DbContext
    /// </summary>
    /// <param name="optionsBuilder">O builder de opções</param>
    protected virtual void ConfigureAdditionalOptions(DbContextOptionsBuilder<TContext> optionsBuilder)
    {
        // Sobrescreva em classes derivadas se necessário
    }

    /// <summary>
    /// Cria a instância do DbContext para operações em tempo de design
    /// </summary>
    /// <param name="args">Argumentos de linha de comando</param>
    /// <returns>Instância configurada do DbContext</returns>
    public TContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        
        // Configura PostgreSQL com opções de migrations
        optionsBuilder.UseNpgsql(GetDesignTimeConnectionString(), options =>
        {
            options.MigrationsAssembly(GetMigrationsAssembly());
            options.MigrationsHistoryTable("__EFMigrationsHistory", GetMigrationsHistorySchema());
        });
        
        // Permite que classes derivadas configurem opções adicionais
        ConfigureAdditionalOptions(optionsBuilder);

        return CreateDbContextInstance(optionsBuilder.Options);
    }
    
    /// <summary>
    /// Cria a instância real do DbContext
    /// Sobrescreva este método para lógica personalizada de construtor
    /// </summary>
    /// <param name="options">As opções configuradas</param>
    /// <returns>Instância do DbContext</returns>
    protected abstract TContext CreateDbContextInstance(DbContextOptions<TContext> options);
}