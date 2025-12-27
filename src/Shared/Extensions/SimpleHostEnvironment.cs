using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Shared.Extensions;

/// <summary>
/// Implementação simples interna de IHostEnvironment para registro de messaging.
/// Para testes, use MockHostEnvironment de MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.
/// </summary>
internal sealed class SimpleHostEnvironment : IHostEnvironment
{
    public SimpleHostEnvironment(string environmentName)
    {
        EnvironmentName = environmentName;
        ApplicationName = "MeAjudaAi";
        ContentRootPath = Directory.GetCurrentDirectory();
    }

    public string EnvironmentName { get; set; }
    public string ApplicationName { get; set; }
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
