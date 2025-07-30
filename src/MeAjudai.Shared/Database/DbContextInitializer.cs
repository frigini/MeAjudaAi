using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudai.Shared.Database;

public sealed class DbContextInitializer(IServiceProvider serviceProvider, ILogger<DbContextInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        // Busca todos os DbContext registrados automaticamente
        var dbContexts = scope.ServiceProvider.GetServices<DbContext>();

        foreach (var context in dbContexts)
        {
            try
            {
                logger.LogInformation("Initializing database for {ContextName}", context.GetType().Name);

                if (context.Database.IsRelational())
                {
                    await context.Database.MigrateAsync(cancellationToken);
                }

                logger.LogInformation("Database initialized successfully for {ContextName}", context.GetType().Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize database for {ContextName}", context.GetType().Name);
                throw;
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}