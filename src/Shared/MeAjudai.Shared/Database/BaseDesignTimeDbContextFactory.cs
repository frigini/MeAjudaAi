using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Base class for design-time DbContext factories across modules
/// Automatically detects module name from namespace
/// </summary>
/// <typeparam name="TContext">The DbContext type</typeparam>
public abstract class BaseDesignTimeDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    /// <summary>
    /// Gets the module name automatically from the derived class namespace
    /// Expected namespace pattern: MeAjudaAi.Modules.{ModuleName}.Infrastructure.Persistence
    /// </summary>
    protected virtual string GetModuleName()
    {
        var derivedType = GetType();
        var namespaceParts = derivedType.Namespace?.Split('.') ?? Array.Empty<string>();
        
        // Look for pattern: MeAjudaAi.Modules.{ModuleName}.Infrastructure
        for (int i = 0; i < namespaceParts.Length - 1; i++)
        {
            if (namespaceParts[i] == "MeAjudaAi" && 
                i + 2 < namespaceParts.Length && 
                namespaceParts[i + 1] == "Modules")
            {
                return namespaceParts[i + 2]; // Return the module name
            }
        }
        
        // Fallback: extract from class name if it follows pattern {ModuleName}DbContextFactory
        var className = derivedType.Name;
        if (className.EndsWith("DbContextFactory"))
        {
            return className.Substring(0, className.Length - "DbContextFactory".Length);
        }
        
        throw new InvalidOperationException(
            $"Cannot determine module name from namespace '{derivedType.Namespace}' or class name '{className}'. " +
            "Expected namespace pattern: 'MeAjudaAi.Modules.{{ModuleName}}.Infrastructure.Persistence' " +
            "or class name pattern: '{{ModuleName}}DbContextFactory'");
    }
    
    /// <summary>
    /// Gets the connection string for design time operations
    /// Can be overridden to provide custom logic
    /// </summary>
    protected virtual string GetDesignTimeConnectionString()
    {
        // Try to get from configuration first
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }
        
        // Fallback to default local development connection
        return GetDefaultConnectionString();
    }
    
    /// <summary>
    /// Gets the migrations assembly name based on module name
    /// </summary>
    protected virtual string GetMigrationsAssembly()
    {
        return $"MeAjudaAi.Modules.{GetModuleName()}.Infrastructure";
    }
    
    /// <summary>
    /// Gets the schema name for migrations history table based on module name
    /// </summary>
    protected virtual string GetMigrationsHistorySchema()
    {
        return GetModuleName().ToLowerInvariant();
    }
    
    /// <summary>
    /// Gets the default connection string for local development
    /// </summary>
    protected virtual string GetDefaultConnectionString()
    {
        var moduleName = GetModuleName().ToLowerInvariant();
        return $"Host=localhost;Database=meajudaai_dev;Username=postgres;Password=dev123;SearchPath={moduleName},public";
    }
    
    /// <summary>
    /// Builds configuration from appsettings files
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
    /// Configure additional options for the DbContext
    /// </summary>
    /// <param name="optionsBuilder">The options builder</param>
    protected virtual void ConfigureAdditionalOptions(DbContextOptionsBuilder<TContext> optionsBuilder)
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Creates the DbContext instance for design time operations
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>The configured DbContext instance</returns>
    public TContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        
        // Configure PostgreSQL with migrations settings
        optionsBuilder.UseNpgsql(GetDesignTimeConnectionString(), options =>
        {
            options.MigrationsAssembly(GetMigrationsAssembly());
            options.MigrationsHistoryTable("__EFMigrationsHistory", GetMigrationsHistorySchema());
        });
        
        // Allow derived classes to configure additional options
        ConfigureAdditionalOptions(optionsBuilder);

        return CreateDbContextInstance(optionsBuilder.Options);
    }
    
    /// <summary>
    /// Creates the actual DbContext instance
    /// Override this method to provide custom constructor logic
    /// </summary>
    /// <param name="options">The configured options</param>
    /// <returns>The DbContext instance</returns>
    protected abstract TContext CreateDbContextInstance(DbContextOptions<TContext> options);
}