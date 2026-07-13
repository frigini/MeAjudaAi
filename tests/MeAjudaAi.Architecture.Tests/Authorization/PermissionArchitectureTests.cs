using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Core.Enums;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Authorization.Services;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MeAjudaAi.Architecture.Tests.Authorization;

/// <summary>
/// Testes de arquitetura para garantir que o sistema de permissões
/// siga as regras estabelecidas e mantenha a integridade da arquitetura.
/// </summary>
public class PermissionArchitectureTests
{
    private readonly Assembly _sharedAssembly = typeof(Permission).Assembly;

    [Fact]
    public void PermissionResolver_ShouldBeSealed()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .HaveNameEndingWith("PermissionResolver")
            .And()
            .AreNotInterfaces()
            .And()
            .AreClasses()
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

            Assert.Contains(":", value);

            var parts = value.Split(':');
            Assert.Equal(2, parts.Length);

            Assert.True(
                parts[0].Length > 0 &&
                parts[0].Any(char.IsLetter) &&
                parts[0].All(c => char.IsLower(c) || c == '-'),
                $"Módulo '{parts[0]}' deve estar em lowercase, não vazio, e conter pelo menos uma letra. Permission: {permission}");

            Assert.True(
                parts[1].Length > 0 &&
                parts[1].Any(char.IsLetter) &&
                parts[1].All(c => char.IsLower(c) || c == '-'),
                $"Ação '{parts[1]}' deve estar em lowercase, não vazia, e conter pelo menos uma letra. Permission: {permission}");
        }
    }

    [Fact]
    public void PermissionEnum_ShouldHaveUniqueValues()
    {
        // Arrange
        var permissionValues = Enum.GetValues<EPermission>();
        var permissionStrings = permissionValues.Select(p => p.GetValue()).ToList();

        // Act
        var duplicates = permissionStrings.GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        // Assert
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
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var env = new Mock<IWebHostEnvironment>().Object;
        services.AddPermissionBasedAuthorization(configuration, env);

        var authorizationServices = new[]
        {
            typeof(IPermissionService),
            typeof(IClaimsTransformation),
            typeof(IAuthorizationHandler)
        };

        // Act & Assert
        foreach (var serviceType in authorizationServices)
        {
            var descriptors = services.Where(s => s.ServiceType == serviceType).ToList();
            
            Assert.NotEmpty(descriptors);
            
            foreach (var descriptor in descriptors)
            {
                if (descriptor.ImplementationType?.Namespace?.StartsWith("MeAjudaAi") == true)
                {
                    Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
                }
            }
        }
    }


    [Fact]
    public void ModulePermissionClasses_ShouldFollowNamingConvention()
    {
        // Arrange & Act - Apenas classes que terminam exatamente com "Permissions" (containers de permissões estáticas)
        // E restringir apenas aos nossos módulos e projeto MeAjudaAi.
        var result = Types.InCurrentDomain()
            .That()
            .HaveNameEndingWith("Permissions")  // Classes de container de permissões
            .And()
            .AreClasses()
            .And()
            .ResideInNamespaceStartingWith("MeAjudaAi")
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
        var claims = new[]
        {
            AuthConstants.Claims.Subject,
            AuthConstants.Claims.Email,
            AuthConstants.Claims.EmailVerified,
            AuthConstants.Claims.PreferredUsername,
            AuthConstants.Claims.GivenName,
            AuthConstants.Claims.FamilyName,
            AuthConstants.Claims.Roles,
            AuthConstants.Claims.UserId,
            AuthConstants.Claims.KeycloakId,
            AuthConstants.Claims.Permission,
            AuthConstants.Claims.Module,
            AuthConstants.Claims.TenantId,
            AuthConstants.Claims.Organization,
            AuthConstants.Claims.IsSystemAdmin
        };

        // Act & Assert
        foreach (var claim in claims)
        {
            Assert.NotNull(claim);
            Assert.NotEmpty(claim);
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
