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

        // Act
        var result = domainAssemblies
            .That()
            .ResideInNamespaceMatching(DomainNamespaceRegex)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespaceRegex)
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
            .ResideInNamespaceMatching(DomainNamespaceRegex)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespaceRegex)
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

        // Act
        var result = applicationAssemblies
            .That()
            .ResideInNamespaceMatching(ApplicationNamespaceRegex)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespaceRegex)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Application layer must not depend on Infrastructure layer. Violations:{Environment.NewLine}" +
            $"{string.Join(Environment.NewLine, result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Infrastructure_Should_Depend_On_Application_Or_Domain()
    {
        // This test verifies that Infrastructure depends on Application or Domain (to implement interfaces/abstractions)
        // Arrange
        var infrastructureAssemblies = Types.InAssemblies(GetAssembliesMatchingPattern("*.Infrastructure.dll"));

        // Act
        var result = infrastructureAssemblies
            .That()
            .ResideInNamespaceMatching(InfrastructureNamespaceRegex)
            .Should()
            .HaveDependencyOnAny(ApplicationNamespaceRegex)
            .Or()
            .HaveDependencyOnAny(DomainNamespaceRegex)
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
    public void GeographicRestrictionMiddleware_Should_Not_Depend_On_Locations_Infrastructure()
    {
        // Verifies that middleware uses interface from Shared, not concrete implementation from Locations
        // Arrange
        var apiAssemblies = GetAssembliesMatchingPattern("MeAjudaAi.ApiService.dll");
        var apiAssembly = apiAssemblies.FirstOrDefault();
        
        if (apiAssembly == null)
        {
            throw new InvalidOperationException("ApiService assembly not found for architecture test");
        }

        var middlewareType = apiAssembly.GetType("MeAjudaAi.Bootstrapper.ApiService.Middlewares.GeographicRestrictionMiddleware");
        if (middlewareType == null)
        {
            throw new InvalidOperationException("GeographicRestrictionMiddleware type not found in ApiService assembly");
        }

        // Act & Assert - Middleware should NOT depend on Locations.Infrastructure
        var constructorParams = middlewareType.GetConstructors()
            .SelectMany(c => c.GetParameters())
            .Select(p => p.ParameterType);

        var hasLocationsInfraDependency = constructorParams.Any(t => 
            t.FullName?.StartsWith("MeAjudaAi.Modules.Locations.Infrastructure") == true);

        hasLocationsInfraDependency.Should().BeFalse(
            "GeographicRestrictionMiddleware must not depend on MeAjudaAi.Modules.Locations.Infrastructure - use IGeographicValidationService from Shared instead");

        // Middleware SHOULD depend on IGeographicValidationService from Shared
        var hasSharedInterfaceDependency = constructorParams.Any(t => 
            t.FullName == "MeAjudaAi.Shared.Geolocation.IGeographicValidationService");

        hasSharedInterfaceDependency.Should().BeTrue(
            "GeographicRestrictionMiddleware must inject IGeographicValidationService from MeAjudaAi.Shared.Geolocation");
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
