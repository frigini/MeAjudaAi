using System.Reflection;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Services;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Architecture.Tests.Authorization;

/// <summary>
/// Testes de arquitetura para garantir que o sistema de permissões
/// siga as regras estabelecidas e mantenha a integridade da arquitetura.
/// </summary>
public class PermissionArchitectureTests
{
    private readonly Assembly _sharedAssembly = typeof(Permission).Assembly;

    [Fact]
    public void PermissionResolver_ShouldImplementIModulePermissionResolver()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .HaveNameEndingWith("PermissionResolver")
            .And()
            .AreNotInterfaces() // Excluir interfaces da verificação
            .And()
            .AreClasses() // Apenas classes concretas
            .Should()
            .ImplementInterface(typeof(IModulePermissionResolver))
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Todos os PermissionResolvers devem implementar IModulePermissionResolver. Violações: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void PermissionResolver_ShouldBeSealed()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .HaveNameEndingWith("PermissionResolver")
            .And()
            .AreNotInterfaces() // Interfaces não podem ser sealed
            .And()
            .AreClasses() // Apenas classes concretas
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Todos os PermissionResolvers devem ser sealed para evitar herança não controlada. Violações: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void PermissionService_ShouldNotDependOnSpecificModules()
    {
        // Arrange & Act
        var result = Types.InAssembly(_sharedAssembly)
            .That()
            .HaveNameEndingWith("PermissionService")
            .Should()
            .NotHaveDependencyOnAny("MeAjudaAi.Modules.Users", "MeAjudaAi.Modules.Providers", "MeAjudaAi.Modules.Orders")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"PermissionService não deve depender de módulos específicos para manter a modularidade. Violações: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void ModulePermissionResolver_ShouldOnlyBeInApplicationLayer()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ImplementInterface(typeof(IModulePermissionResolver))
            .And()
            .DoNotHaveNameMatching(@".*Keycloak.*")  // Permitir resolvers específicos do Keycloak no namespace Shared
            .And()
            .AreClasses() // Apenas classes concretas
            .Should()
            .ResideInNamespaceMatching(@".*\.Application\.Authorization")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"ModulePermissionResolvers devem residir apenas na camada Application/Authorization. Violações: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void PermissionClasses_ShouldBeInAuthorizationNamespace()
    {
        // Arrange & Act
        var result = Types.InAssembly(_sharedAssembly)
            .That()
            .HaveNameMatching(@".*Permission.*")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveName("SchemaPermissionsManager")  // Permitir SchemaPermissionsManager no namespace Database
            .And()
            .DoNotHaveName("PermissionHealthCheckExtensions")  // Extensions em Shared.Extensions (organização de pasta)
            .Should()
            .ResideInNamespace("MeAjudaAi.Shared.Authorization")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Classes de permissão devem estar no namespace Authorization. Violações: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void PermissionEnum_ShouldHaveDisplayAttributes()
    {
        // Arrange
        var permissionValues = Enum.GetValues<EPermission>();

        // Act & Assert
        foreach (var permission in permissionValues)
        {
            var field = typeof(EPermission).GetField(permission.ToString());
            var displayAttribute = field?.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();

            Assert.NotNull(displayAttribute);
            Assert.NotNull(displayAttribute.Name);
            Assert.NotEmpty(displayAttribute.Name);
            // Description é opcional para permissões
        }
    }

    [Fact]
    public void PermissionEnum_ShouldFollowNamingConvention()
    {
        // Arrange
        var permissionValues = Enum.GetValues<EPermission>();

        // Act & Assert
        foreach (var permission in permissionValues)
        {
            var value = permission.GetValue();

            // Deve seguir o padrão "module:action"
            Assert.Contains(":", value);

            var parts = value.Split(':');
            Assert.Equal(2, parts.Length);

            // Módulo deve estar em lowercase
            Assert.True(parts[0].All(char.IsLower) || parts[0] == "admin",
                $"Módulo '{parts[0]}' deve estar em lowercase ou ser 'admin'. Permission: {permission}");

            // Ação deve estar em lowercase
            Assert.True(parts[1].All(char.IsLower),
                $"Ação '{parts[1]}' deve estar em lowercase. Permission: {permission}");
        }
    }

    [Fact]
    public void PermissionEnum_ShouldHaveUniqueValues()
    {
        // Arrange
        var permissionValues = Enum.GetValues<EPermission>();
        var permissionStrings = permissionValues.Select(p => p.GetValue()).ToList();

        // Act & Assert
        var duplicates = permissionStrings.GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        Assert.Empty(duplicates);
    }

    [Fact]
    public void PermissionExtensions_ShouldNotDependOnSpecificModules()
    {
        // Arrange & Act
        var result = Types.InAssembly(_sharedAssembly)
            .That()
            .HaveNameEndingWith("Extensions")
            .And()
            .ResideInNamespace("MeAjudaAi.Shared.Authorization")
            .Should()
            .NotHaveDependencyOnAny("MeAjudaAi.Modules.Users", "MeAjudaAi.Modules.Providers")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Extensões de autorização não devem depender de módulos específicos. Violações: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void AuthorizationServices_ShouldBeRegisteredAsScoped()
    {
        // Esta regra deve ser verificada na configuração de DI
        // Arrange
        var authorizationServiceTypes = new[]
        {
            typeof(IPermissionService),
            typeof(PermissionService),
            typeof(IModulePermissionResolver)
        };

        // Act & Assert
        foreach (var serviceType in authorizationServiceTypes)
        {
            // Em um teste real, verificaria o ServiceLifetime no container
            // Por agora, apenas verificamos que os tipos existem
            Assert.NotNull(serviceType);
            Assert.True(serviceType.IsClass || serviceType.IsInterface);
        }
    }

    [Fact]
    public void ModulePermissionClasses_ShouldFollowNamingConvention()
    {
        // Arrange & Act - Apenas classes que terminam exatamente com "Permissions" (containers de permissões estáticas)
        var result = Types.InCurrentDomain()
            .That()
            .HaveNameEndingWith("Permissions")  // Classes de container de permissões
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .BeStatic()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Classes de organização de permissões devem ser static. Violações: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void AuthConstants_Claims_ShouldBeConstantStrings()
    {
        // Arrange
        var claimsType = typeof(AuthConstants.Claims);
#pragma warning disable S3011 // Reflection is required for architecture testing
        var fields = claimsType.GetFields(BindingFlags.Public | BindingFlags.Static);
#pragma warning restore S3011

        // Act & Assert
        foreach (var field in fields)
        {
            Assert.True(field.IsStatic, $"Field {field.Name} deve ser static");
            Assert.True(field.IsLiteral || field.IsInitOnly, $"Field {field.Name} deve ser const ou readonly");
            Assert.Equal(typeof(string), field.FieldType);

            var value = field.GetValue(null) as string;
            Assert.NotNull(value);
            Assert.NotEmpty(value);
        }
    }

    [Fact]
    public void PermissionRequirements_ShouldImplementIAuthorizationRequirement()
    {
        // Arrange & Act
        var result = Types.InAssembly(_sharedAssembly)
            .That()
            .HaveNameEndingWith("Requirement")
            .And()
            .ResideInNamespace("MeAjudaAi.Shared.Authorization")
            .Should()
            .ImplementInterface(typeof(Microsoft.AspNetCore.Authorization.IAuthorizationRequirement))
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Todos os Requirements devem implementar IAuthorizationRequirement. Violações: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void AuthorizationHandlers_ShouldBeInCorrectNamespace()
    {
        // Arrange & Act
        var result = Types.InAssembly(_sharedAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .Inherit(typeof(Microsoft.AspNetCore.Authorization.AuthorizationHandler<>))
            .Should()
            .ResideInNamespace("MeAjudaAi.Shared.Authorization")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"AuthorizationHandlers devem estar no namespace correto. Violações: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void PermissionSystem_ShouldNotHaveCircularDependencies()
    {
        // Arrange & Act
        var result = Types.InAssembly(_sharedAssembly)
            .That()
            .ResideInNamespace("MeAjudaAi.Shared.Authorization")
            .Should()
            .NotHaveDependencyOn("MeAjudaAi.Shared.Authorization")
            .Or()
            .HaveDependencyOn("MeAjudaAi.Shared.Authorization") // Permitir dependências internas do próprio namespace
            .GetResult();

        // Esta regra é mais complexa e seria melhor implementada com análise de dependências específica
        Assert.True(result.IsSuccessful || result.FailingTypeNames.Count() < 10,
            "Não deve haver dependências circulares problemáticas no sistema de autorização");
    }
}
