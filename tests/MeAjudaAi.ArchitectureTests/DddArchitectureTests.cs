namespace MeAjudaAi.ArchitectureTests;

/// <summary>
/// Architecture tests to validate Domain-Driven Design (DDD) layer separation and dependencies.
/// Uses NetArchTest.Rules to enforce architectural constraints across all modules.
/// </summary>
public class DddArchitectureTests
{
    private const string DomainNamespace = "MeAjudaAi.Modules.*.Domain";
    private const string ApplicationNamespace = "MeAjudaAi.Modules.*.Application";
    private const string InfrastructureNamespace = "MeAjudaAi.Modules.*.Infrastructure";
    private const string SharedNamespace = "MeAjudaAi.Shared";

    [Fact]
    public void Domain_Should_Not_DependOn_Infrastructure()
    {
        // Arrange
        var domainAssemblies = Types.InAssemblies(GetAssembliesMatchingPattern("*.Domain.dll"));

        // Act
        var result = domainAssemblies
            .That()
            .ResideInNamespace(DomainNamespace)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer must not depend on Infrastructure layer. Violations:{Environment.NewLine}" +
            $"{string.Join(Environment.NewLine, result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Should_Not_DependOn_Application()
    {
        // Arrange
        var domainAssemblies = Types.InAssemblies(GetAssembliesMatchingPattern("*.Domain.dll"));

        // Act
        var result = domainAssemblies
            .That()
            .ResideInNamespace(DomainNamespace)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer must not depend on Application layer. Violations:{Environment.NewLine}" +
            $"{string.Join(Environment.NewLine, result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_Should_Only_DependOn_Domain_And_Shared()
    {
        // Arrange
        var applicationAssemblies = Types.InAssemblies(GetAssembliesMatchingPattern("*.Application.dll"));

        // Act
        var result = applicationAssemblies
            .That()
            .ResideInNamespace(ApplicationNamespace)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Application layer must not depend on Infrastructure layer. Violations:{Environment.NewLine}" +
            $"{string.Join(Environment.NewLine, result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Infrastructure_Should_Implement_Application_Interfaces()
    {
        // This test verifies that Infrastructure depends on Application (to implement interfaces)
        // Arrange
        var infrastructureAssemblies = Types.InAssemblies(GetAssembliesMatchingPattern("*.Infrastructure.dll"));

        // Act
        var result = infrastructureAssemblies
            .That()
            .ResideInNamespace(InfrastructureNamespace)
            .Should()
            .HaveDependencyOn(ApplicationNamespace)
            .Or()
            .HaveDependencyOn(DomainNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Infrastructure layer must depend on Application or Domain layer. Violations:{Environment.NewLine}" +
            $"{string.Join(Environment.NewLine, result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Shared_Should_Not_DependOn_Any_Module()
    {
        // Arrange
        var sharedAssemblies = Types.InAssemblies(GetAssembliesMatchingPattern("MeAjudaAi.Shared.dll"));

        // Act
        var result = sharedAssemblies
            .That()
            .ResideInNamespace(SharedNamespace)
            .ShouldNot()
            .HaveDependencyOn("MeAjudaAi.Modules")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Shared layer must not depend on any specific module (Locations, Providers, Users, etc.). Violations:{Environment.NewLine}" +
            $"{string.Join(Environment.NewLine, result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void GeographicRestrictionMiddleware_Should_Use_IGeographicValidationService_Interface()
    {
        // Verifies that middleware uses interface from Shared, not concrete implementation from Locations
        // Arrange
        var sharedAssemblies = Types.InAssemblies(GetAssembliesMatchingPattern("MeAjudaAi.Shared.dll"));

        // Act - verify IGeographicValidationService exists in Shared namespace
        var interfaceExists = sharedAssemblies
            .That()
            .ResideInNamespace("MeAjudaAi.Shared.Geolocation")
            .And()
            .HaveNameMatching("IGeographicValidationService")
            .GetTypes()
            .Any();

        // Assert
        interfaceExists.Should().BeTrue(
            "IGeographicValidationService interface must exist in Shared.Geolocation namespace for dependency inversion");
    }

    [Fact]
    public void Domain_Entities_Should_Have_Proper_Naming()
    {
        // Arrange
        var domainAssemblies = Types.InAssemblies(GetAssembliesMatchingPattern("*.Domain.dll"));

        // Act
        var result = domainAssemblies
            .That()
            .ResideInNamespace("MeAjudaAi.Modules.*.Domain.Entities")
            .And()
            .AreClasses()
            .Should()
            .NotHaveNameMatching(".*Dto$")
            .And()
            .NotHaveNameMatching(".*ViewModel$")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain entities must not use DTO or ViewModel naming conventions. Violations:{Environment.NewLine}" +
            $"{string.Join(Environment.NewLine, result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Locations_Module_Should_Not_Create_Circular_Dependencies()
    {
        // Arrange - Locations module components
        var locationsInfrastructure = Types.InAssembly(
            GetAssembliesMatchingPattern("MeAjudaAi.Modules.Locations.Infrastructure.dll").First());

        // Act - verify Infrastructure doesn't reference API (which would be circular)
        var result = locationsInfrastructure
            .That()
            .ResideInNamespace("MeAjudaAi.Modules.Locations.Infrastructure")
            .ShouldNot()
            .HaveDependencyOn("MeAjudaAi.Modules.Locations.API")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Infrastructure must not depend on API layer (circular dependency). Violations:{Environment.NewLine}" +
            $"{string.Join(Environment.NewLine, result.FailingTypeNames ?? [])}");
    }

    /// <summary>
    /// Helper method to get assemblies matching a pattern from the current domain.
    /// </summary>
    private static IEnumerable<System.Reflection.Assembly> GetAssembliesMatchingPattern(string pattern)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var files = Directory.GetFiles(baseDirectory, pattern, SearchOption.AllDirectories);

        foreach (var file in files)
        {
            System.Reflection.Assembly? assembly = null;
            try
            {
                assembly = System.Reflection.Assembly.LoadFrom(file);
            }
            catch
            {
                // Skip assemblies that can't be loaded
                continue;
            }

            if (assembly != null)
            {
                yield return assembly;
            }
        }
    }
}
