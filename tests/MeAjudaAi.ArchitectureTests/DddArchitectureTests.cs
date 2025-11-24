namespace MeAjudaAi.ArchitectureTests;

/// <summary>
/// Architecture tests to validate Domain-Driven Design (DDD) layer separation and dependencies.
/// Uses NetArchTest.Rules to enforce architectural constraints across all modules.
/// </summary>
public class DddArchitectureTests
{
    private const string DomainNamespaceRegex = @"^MeAjudaAi\.Modules\..*\.Domain($|\.)";
    private const string ApplicationNamespaceRegex = @"^MeAjudaAi\.Modules\..*\.Application($|\.)";
    private const string InfrastructureNamespaceRegex = @"^MeAjudaAi\.Modules\..*\.Infrastructure($|\.)";
    private const string SharedNamespaceRegex = @"^MeAjudaAi\.Shared($|\.)";

    [Fact]
    public void Domain_Should_Not_DependOn_Infrastructure()
    {
        // Arrange
        var domainAssemblies = Types.InAssemblies(GetAssembliesMatchingPattern("*.Domain.dll"));
        var infrastructureNamespaces = GetModuleNamespaces("Infrastructure");

        // Act
        var result = domainAssemblies
            .That()
            .ResideInNamespaceMatching(DomainNamespaceRegex)
            .ShouldNot()
            .HaveDependencyOnAny(infrastructureNamespaces.ToArray())
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
        var applicationNamespaces = GetModuleNamespaces("Application");

        // Act
        var result = domainAssemblies
            .That()
            .ResideInNamespaceMatching(DomainNamespaceRegex)
            .ShouldNot()
            .HaveDependencyOnAny(applicationNamespaces.ToArray())
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer must not depend on Application layer. Violations:{Environment.NewLine}" +
            $"{string.Join(Environment.NewLine, result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_Should_Not_DependOn_Infrastructure()
    {
        // Arrange
        var applicationAssemblies = Types.InAssemblies(GetAssembliesMatchingPattern("*.Application.dll"));
        var infrastructureNamespaces = GetModuleNamespaces("Infrastructure");

        // Act
        var result = applicationAssemblies
            .That()
            .ResideInNamespaceMatching(ApplicationNamespaceRegex)
            .ShouldNot()
            .HaveDependencyOnAny(infrastructureNamespaces.ToArray())
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Application layer must not depend on Infrastructure layer. Violations:{Environment.NewLine}" +
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
            .ResideInNamespaceMatching(SharedNamespaceRegex)
            .ShouldNot()
            .HaveDependencyOn("MeAjudaAi.Modules")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Shared layer must not depend on any specific module (Locations, Providers, Users, etc.). Violations:{Environment.NewLine}" +
            $"{string.Join(Environment.NewLine, result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Entities_Should_Have_Proper_Naming()
    {
        // Arrange
        var domainAssemblies = Types.InAssemblies(GetAssembliesMatchingPattern("*.Domain.dll"));

        // Act
        var result = domainAssemblies
            .That()
            .ResideInNamespaceMatching(@"^MeAjudaAi\.Modules\..*\.Domain\.Entities($|\.)")
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
            catch (Exception ex) when (
                ex is BadImageFormatException or
                FileLoadException or
                FileNotFoundException)
            {
                // Skip assemblies that can't be loaded (native DLLs, missing dependencies, etc.)
                continue;
            }

            if (assembly != null)
            {
                yield return assembly;
            }
        }
    }

    /// <summary>
    /// Gets concrete module namespaces for the specified layer (Domain, Application, Infrastructure).
    /// HaveDependencyOnAny requires exact namespace strings, not regex patterns.
    /// </summary>
    private static List<string> GetModuleNamespaces(string layer)
    {
        var modules = new[] { "Locations", "Providers", "Users", "Documents", "ServiceCatalogs", "SearchProviders" };
        return modules.Select(module => $"MeAjudaAi.Modules.{module}.{layer}").ToList();
    }
}
