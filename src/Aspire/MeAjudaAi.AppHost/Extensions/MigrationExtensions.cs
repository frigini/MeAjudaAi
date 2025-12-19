using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using MeAjudaAi.AppHost.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.AppHost.Extensions;

/// <summary>
/// Extensões para aplicar migrations automaticamente no Aspire
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Adiciona e executa migrations de todos os módulos antes de iniciar a aplicação
    /// </summary>
    public static IResourceBuilder<T> WithMigrations<T>(
        this IResourceBuilder<T> builder) where T : IResourceWithConnectionString
    {
        builder.ApplicationBuilder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, MigrationHostedService>());
        return builder;
    }
}
