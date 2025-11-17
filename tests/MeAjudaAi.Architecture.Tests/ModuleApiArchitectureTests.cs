using System.Reflection;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Contracts.Modules.Location;
using MeAjudaAi.Shared.Contracts.Modules.Providers;
using MeAjudaAi.Shared.Contracts.Modules.Users;
using MeAjudaAi.Shared.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Architecture.Tests;

public class ModuleApiArchitectureTests
{
    [Fact]
    public void ModuleApiInterfaces_ShouldBeInSharedContractsNamespace()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(IUsersModuleApi).Assembly)
            .That()
            .AreInterfaces()
            .And()
            .HaveNameEndingWith("ModuleApi")
            .Should()
            .ResideInNamespace("MeAjudaAi.Shared.Contracts.Modules")
            .Or()
            .ResideInNamespaceMatching(@"MeAjudaAi\.Shared\.Contracts\.Modules\.\w+");

        // Assert
        var violations = result.GetResult().FailingTypes;
        violations?.Should().BeEmpty(
                because: "Module API interfaces should be in the Shared.Contracts.Modules namespace hierarchy");
    }

    [Fact]
    public void ModuleApiImplementations_ShouldHaveModuleApiAttribute()
    {
        // Arrange
        var assemblies = GetModuleAssemblies();

        // Act & Assert
        foreach (var assembly in assemblies)
        {
            // Obtém os tipos que implementam IModuleApi
            var moduleApiTypes = Types.InAssembly(assembly)
                .That()
                .AreClasses()
                .And()
                .ImplementInterface(typeof(IModuleApi))
                .GetTypes();

            foreach (var type in moduleApiTypes)
            {
                // Verifica se possui o atributo ModuleApi
                var attribute = type.GetCustomAttribute<ModuleApiAttribute>();
                attribute.Should().NotBeNull(
                    because: $"Module API implementation {type.Name} should have [ModuleApi] attribute");

                attribute!.ModuleName.Should().NotBeNullOrWhiteSpace(
                    because: $"Module API {type.Name} should have a valid module name");

                attribute.ApiVersion.Should().NotBeNullOrWhiteSpace(
                    because: $"Module API {type.Name} should have a valid API version");
            }
        }
    }

    [Fact]
    public void ModuleApiMethods_ShouldReturnResultType()
    {
        // Arrange
        var moduleApiTypes = Types.InAssembly(typeof(IUsersModuleApi).Assembly)
            .That()
            .AreInterfaces()
            .And()
            .HaveNameEndingWith("ModuleApi")
            .GetTypes();

        // Act & Assert
        foreach (var type in moduleApiTypes)
        {
            // Verifica os métodos das interfaces
            var methods = type.GetMethods()
                .Where(m => !m.IsSpecialName && m.DeclaringType == type)
                .Where(m => m.Name != nameof(IModuleApi.IsAvailableAsync)); // Exclui métodos da interface base

            foreach (var method in methods)
            {
                if (method.ReturnType.IsGenericType)
                {
                    var genericType = method.ReturnType.GetGenericTypeDefinition();

                    if (genericType == typeof(Task<>))
                    {
                        var taskInnerType = method.ReturnType.GetGenericArguments()[0];

                        if (taskInnerType.IsGenericType)
                        {
                            var innerGenericType = taskInnerType.GetGenericTypeDefinition();
                            innerGenericType.Should().Be(typeof(Result<>),
                                because: $"Async Module API method {type.Name}.{method.Name} should return Task<Result<T>>");
                        }
                    }
                }
            }
        }
    }

    [Fact]
    public void ModuleApiMethods_ShouldHaveCancellationTokenParameter()
    {
        // Arrange
        var moduleApiTypes = Types.InAssembly(typeof(IUsersModuleApi).Assembly)
            .That()
            .AreInterfaces()
            .And()
            .HaveNameEndingWith("ModuleApi")
            .GetTypes();

        // Act & Assert
        foreach (var type in moduleApiTypes)
        {
            // Verifica métodos assíncronos
            var methods = type.GetMethods()
                .Where(m => !m.IsSpecialName && m.DeclaringType == type)
                .Where(m => m.ReturnType.IsGenericType &&
                           m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));

            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                var hasCancellationToken = parameters.Any(p => p.ParameterType == typeof(CancellationToken));

                hasCancellationToken.Should().BeTrue(
                    because: $"Async method {type.Name}.{method.Name} should have a CancellationToken parameter");

                // Verifica se possui valor padrão
                var cancellationParam = parameters.FirstOrDefault(p => p.ParameterType == typeof(CancellationToken));
                cancellationParam?.HasDefaultValue.Should().BeTrue(
                        because: $"CancellationToken parameter in {type.Name}.{method.Name} should have default value");
            }
        }
    }

    [Fact]
    public void ModuleApiDtos_ShouldBeRecords()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(ModuleUserDto).Assembly)
            .That()
            .ResideInNamespaceMatching(@"MeAjudaAi\.Shared\.Contracts\.Modules\.\w+")
            .And()
            .HaveNameEndingWith("Dto")
            .Should()
            .BeSealed()
            .And()
            .BeClasses(); // Records são classes em .NET

        // Assert
        var violations = result.GetResult().FailingTypes;
        violations?.Should().BeEmpty(
                because: "Module API DTOs should be sealed records for immutability");
    }

    [Fact]
    public void ModuleApiImplementations_ShouldNotDependOnOtherModules()
    {
        // Arrange
        var assemblies = GetModuleAssemblies();

        // Act & Assert
        foreach (var assembly in assemblies)
        {
            var moduleName = GetModuleName(assembly);

            // Verifica dependências entre módulos
            var result = Types.InAssembly(assembly)
                .That()
                .ImplementInterface(typeof(IModuleApi))
                .Should()
                .NotHaveDependencyOnAny(GetOtherModuleNamespaces(moduleName));

            var violations = result.GetResult().FailingTypes;
            violations?.Should().BeEmpty(
                    because: $"Module API in {moduleName} should not depend on other modules");
        }
    }

    [Fact]
    public void ModuleApiContracts_ShouldNotReferenceInternalModuleTypes()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(IUsersModuleApi).Assembly)
            .That()
            .ResideInNamespace("MeAjudaAi.Shared.Contracts.Modules")
            .Should()
            .NotHaveDependencyOnAny("MeAjudaAi.Modules.*.Domain", "MeAjudaAi.Modules.*.Infrastructure");

        // Assert
        var violations = result.GetResult().FailingTypes;
        if (violations != null)
        {
            violations.Should().BeEmpty(
                because: "Module API contracts should not reference internal module types");
        }
    }

    [Fact]
    public void ModuleApiImplementations_ShouldBeSealed()
    {
        // Arrange
        var assemblies = GetModuleAssemblies();

        // Act & Assert
        foreach (var assembly in assemblies)
        {
            // Verifica se as implementações são sealed
            var result = Types.InAssembly(assembly)
                .That()
                .ImplementInterface(typeof(IModuleApi))
                .Should()
                .BeSealed();

            var violations = result.GetResult().FailingTypes;
            violations?.Should().BeEmpty(
                    because: "Module API implementations should be sealed to prevent inheritance");
        }
    }

    [Fact]
    public void IUsersModuleApi_ShouldHaveAllEssentialMethods()
    {
        // Arrange
        var type = typeof(IUsersModuleApi);

        // Act
        var methods = type.GetMethods()
            .Where(m => !m.IsSpecialName && m.DeclaringType == type)
            .Select(m => m.Name)
            .ToList();

        // Assert
        methods.Should().Contain("GetUserByIdAsync", because: "Should allow getting user by ID");
        methods.Should().Contain("GetUserByEmailAsync", because: "Should allow getting user by email");
        methods.Should().Contain("UserExistsAsync", because: "Should allow checking if user exists");
        methods.Should().Contain("EmailExistsAsync", because: "Should allow checking if email exists");
        methods.Should().Contain("GetUsersBatchAsync", because: "Should allow batch operations");
    }

    [Fact]
    public void IProvidersModuleApi_ShouldHaveAllEssentialMethods()
    {
        // Arrange
        var type = typeof(IProvidersModuleApi);

        // Act
        var methods = type.GetMethods()
            .Where(m => !m.IsSpecialName && m.DeclaringType == type)
            .Select(m => m.Name)
            .ToList();

        // Assert
        methods.Should().Contain("GetProviderByIdAsync", because: "Should allow getting provider by ID");
        methods.Should().Contain("GetProviderByUserIdAsync", because: "Should allow getting provider by user ID");
        methods.Should().Contain("ProviderExistsAsync", because: "Should allow checking if provider exists");
        methods.Should().Contain("UserIsProviderAsync", because: "Should allow checking if user is provider");
        methods.Should().Contain("GetProvidersBatchAsync", because: "Should allow batch operations");
        methods.Should().Contain("GetProvidersByCityAsync", because: "Should allow getting providers by city");
        methods.Should().Contain("GetProvidersByStateAsync", because: "Should allow getting providers by state");
        methods.Should().Contain("GetProvidersByTypeAsync", because: "Should allow getting providers by type");
        methods.Should().Contain("GetProvidersByVerificationStatusAsync", because: "Should allow getting providers by verification status");
    }

    [Fact]
    public void ILocationModuleApi_ShouldHaveAllEssentialMethods()
    {
        // Arrange
        var type = typeof(ILocationModuleApi);

        // Act
        var methods = type.GetMethods()
            .Where(m => !m.IsSpecialName && m.DeclaringType == type)
            .Select(m => m.Name)
            .ToList();

        // Assert
        methods.Should().Contain("GetAddressFromCepAsync", because: "Should allow getting address from CEP");
        methods.Should().Contain("GetCoordinatesFromAddressAsync", because: "Should allow geocoding addresses");
    }

    private static Assembly[] GetModuleAssemblies()
    {
        // Obtém todos os assemblies que possuem implementações de Module API
        return [.. AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.Contains("MeAjudaAi.Modules") == true)
            .Where(a => a.FullName?.Contains("Application") == true)];
    }

    private static string GetModuleName(Assembly assembly)
    {
        // Extrai o nome do módulo do nome do assembly
        var name = assembly.GetName().Name ?? "";
        var parts = name.Split('.');
        return parts.Length >= 3 ? parts[2] : "Unknown"; // MeAjudaAi.Modules.{ModuleName}
    }

    private static string[] GetOtherModuleNamespaces(string currentModule)
    {
        // Dynamically discover all modules from loaded assemblies
        var allModules = GetModuleAssemblies()
            .Select(GetModuleName)
            .Distinct()
            .ToArray();
        return [.. allModules
            .Where(m => m != currentModule)
            .Select(m => $"MeAjudaAi.Modules.{m}")];
    }
}
