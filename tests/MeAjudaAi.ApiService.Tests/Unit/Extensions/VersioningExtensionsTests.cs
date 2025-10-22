using FluentAssertions;

namespace MeAjudaAi.ApiService.Tests.Unit.Extensions;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class VersioningExtensionsTests
{
    [Fact]
    public void VersioningExtensions_ShouldBeValidClass()
    {
        // Arrange & Act
        var extensionsType = typeof(MeAjudaAi.ApiService.Extensions.VersioningExtensions);

        // Assert
        extensionsType.Should().NotBeNull();
        extensionsType.IsClass.Should().BeTrue();
        extensionsType.IsAbstract.Should().BeTrue(); // Static classes are abstract and sealed
        extensionsType.IsSealed.Should().BeTrue();
    }

    [Fact]
    public void AddApiVersioning_ExtensionMethod_ShouldExist()
    {
        // Arrange
        var extensionsType = typeof(MeAjudaAi.ApiService.Extensions.VersioningExtensions);

        // Act
        var methods = extensionsType.GetMethods();
        var addApiVersioningMethod = methods.FirstOrDefault(m => m.Name == "AddApiVersioning");

        // Assert
        addApiVersioningMethod.Should().NotBeNull();
        addApiVersioningMethod!.IsStatic.Should().BeTrue();
        addApiVersioningMethod.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void VersioningExtensions_ShouldHaveCorrectNamespace()
    {
        // Arrange & Act
        var extensionsType = typeof(MeAjudaAi.ApiService.Extensions.VersioningExtensions);

        // Assert
        extensionsType.Namespace.Should().Be("MeAjudaAi.ApiService.Extensions");
    }

    [Fact]
    public void VersioningExtensions_ShouldBeInCorrectAssembly()
    {
        // Arrange & Act
        var extensionsType = typeof(MeAjudaAi.ApiService.Extensions.VersioningExtensions);

        // Assert
        extensionsType.Assembly.Should().NotBeNull();
        extensionsType.Assembly.GetName().Name.Should().Be("MeAjudaAi.ApiService");
    }
}
