using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Extensions;

/// <summary>
/// Implementação simples interna de IHostEnvironment para registro de messaging.
/// Para testes, use MockHostEnvironment de MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class SimpleHostEnvironment(string environmentName) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = environmentName;
    public string ApplicationName { get; set; } = "MeAjudaAi";
    public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
