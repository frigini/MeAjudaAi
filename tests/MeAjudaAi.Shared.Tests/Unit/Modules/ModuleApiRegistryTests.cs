using FluentAssertions;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Modules;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Modules;

[Trait("Category", "Unit")]
public class ModuleApiRegistryTests
{
    [Fact]
    public void AddModuleApis_WithNoAssemblies_ShouldUseCallingAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddModuleApis();

        // Assert
        services.Should().NotBeNull();
    }

    [Fact]
    public void AddModuleApis_WithValidModule_ShouldRegisterModuleApi()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestModule).Assembly;

        // Act
        services.AddModuleApis(assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        var moduleApis = provider.GetServices<IModuleApi>().ToList();
        moduleApis.Should().NotBeEmpty();
        moduleApis.Should().Contain(m => m.GetType() == typeof(TestModule));
    }

    [Fact]
    public void AddModuleApis_WithValidModule_ShouldRegisterTypedInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestModule).Assembly;

        // Act
        services.AddModuleApis(assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        var testModuleApi = provider.GetService<ITestModuleApi>();
        testModuleApi.Should().NotBeNull();
        testModuleApi.Should().BeAssignableTo<ITestModuleApi>();
    }

    [Fact]
    public void AddModuleApis_WithMultipleAssemblies_ShouldRegisterAllModules()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly1 = typeof(TestModule).Assembly;
        var assembly2 = typeof(AnotherTestModule).Assembly;

        // Act
        services.AddModuleApis(assembly1, assembly2);

        // Assert
        var provider = services.BuildServiceProvider();
        var modules = provider.GetServices<IModuleApi>().ToList();
        modules.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void AddModuleApis_WithAbstractModule_ShouldNotRegister()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(AbstractTestModule).Assembly;

        // Act
        services.AddModuleApis(assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        var modules = provider.GetServices<IModuleApi>().ToList();
        modules.Should().NotContain(m => m.GetType() == typeof(AbstractTestModule));
    }

    [Fact]
    public void AddModuleApis_WithModuleWithoutAttribute_ShouldNotRegister()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(ModuleWithoutAttribute).Assembly;

        // Act
        services.AddModuleApis(assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        var modules = provider.GetServices<IModuleApi>().ToList();
        modules.Should().NotContain(m => m.GetType() == typeof(ModuleWithoutAttribute));
    }

    [Fact]
    public void AddModuleApis_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddModuleApis();

        // Assert
        result.Should().BeSameAs(services);
    }

    #region Test Helpers

    public interface ITestModuleApi : IModuleApi
    {
        string GetTestData();
    }

    [ModuleApi("test-module", "v1")]
    public class TestModule : ITestModuleApi
    {
        public string ModuleName => "test-module";
        public string ApiVersion => "v1";

        public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public string GetTestData() => "test";
    }

    [ModuleApi("another-module", "v1")]
    public class AnotherTestModule : IModuleApi
    {
        public string ModuleName => "another-module";
        public string ApiVersion => "v1";

        public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }

    public abstract class AbstractTestModule : IModuleApi
    {
        public abstract string ModuleName { get; }
        public abstract string ApiVersion { get; }

        public abstract Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    }

    public class ModuleWithoutAttribute : IModuleApi
    {
        public string ModuleName => "no-attribute";
        public string ApiVersion => "v1";

        public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }

    #endregion
}
