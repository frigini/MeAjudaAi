using Microsoft.Extensions.DependencyInjection;
using MeAjudaAi.Tests.Shared.Constants;

namespace MeAjudaAi.Shared.Tests.Extensions;

/// <summary>
/// Extensões para simplificar configuração de testes comuns
/// </summary>
public static class TestConfigurationExtensions
{
    /// <summary>
    /// Configura timeouts padrão para testes
    /// </summary>
    public static IServiceCollection AddTestTimeouts(this IServiceCollection services)
    {
        services.Configure<TestTimeoutOptions>(options =>
        {
            options.ShortTimeout = TestData.Performance.ShortTimeout;
            options.MediumTimeout = TestData.Performance.MediumTimeout;
            options.LongTimeout = TestData.Performance.LongTimeout;
        });
        
        return services;
    }
    
    /// <summary>
    /// Adiciona configurações de paginação para testes
    /// </summary>
    public static IServiceCollection AddTestPagination(this IServiceCollection services)
    {
        services.Configure<TestPaginationOptions>(options =>
        {
            options.DefaultPageSize = TestData.Pagination.DefaultPageSize;
            options.MaxPageSize = TestData.Pagination.MaxPageSize;
            options.FirstPage = TestData.Pagination.FirstPage;
        });
        
        return services;
    }
}

/// <summary>
/// Opções para timeouts de teste
/// </summary>
public class TestTimeoutOptions
{
    public TimeSpan ShortTimeout { get; set; }
    public TimeSpan MediumTimeout { get; set; }
    public TimeSpan LongTimeout { get; set; }
}

/// <summary>
/// Opções para paginação em testes
/// </summary>
public class TestPaginationOptions
{
    public int DefaultPageSize { get; set; }
    public int MaxPageSize { get; set; }
    public int FirstPage { get; set; }
}