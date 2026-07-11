using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks;

/// <summary>
/// Implementação mock de IHostEnvironment para fins de teste.
/// </summary>
public class MockHostEnvironment(string environmentName = "Testing") : IHostEnvironment
{
    public string EnvironmentName { get; set; } = environmentName;
    public string ApplicationName { get; set; } = "MeAjudaAi.Tests";
    public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
