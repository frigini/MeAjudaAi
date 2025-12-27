using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks;

/// <summary>
/// Mock implementation of IHostEnvironment for testing purposes.
/// </summary>
public class MockHostEnvironment : IHostEnvironment
{
    public MockHostEnvironment(string environmentName = "Testing")
    {
        EnvironmentName = environmentName;
        ApplicationName = "MeAjudaAi.Tests";
        ContentRootPath = Directory.GetCurrentDirectory();
    }

    public string EnvironmentName { get; set; }
    public string ApplicationName { get; set; }
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
