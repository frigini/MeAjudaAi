using System.Reflection;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Keycloak;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

public class KeycloakPermissionResolverTests
{
    private readonly Mock<IConfiguration> _configurationMock;

    public KeycloakPermissionResolverTests()
    {
        var configSectionMock = new Mock<IConfigurationSection>();
        configSectionMock.Setup(x => x.Value).Returns("http://localhost");
        
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x.GetSection("Keycloak")).Returns(configSectionMock.Object);
    }

    [Fact]
    public void MapKeycloakRoleToPermissions_AdminRole_ShouldHaveCriticalPermissions()
    {
        // Arrange
        var resolver = CreateResolver();
        var adminRole = "admin";

        // Act
        var permissions = resolver.MapKeycloakRoleToPermissions(adminRole).ToList();

        // Assert
        Assert.Contains(EPermission.AdminSystem, permissions);
        Assert.Contains(EPermission.AdminUsers, permissions);
        
        // Users
        Assert.Contains(EPermission.UsersRead, permissions);
        Assert.Contains(EPermission.UsersList, permissions);
        
        // Providers
        Assert.Contains(EPermission.ProvidersRead, permissions);
        Assert.Contains(EPermission.ProvidersList, permissions); // This assertion ensures the fix
        
        // Service Catalogs
        Assert.Contains(EPermission.ServiceCatalogsRead, permissions);
        Assert.Contains(EPermission.ServiceCatalogsManage, permissions);
        
        // Locations
        Assert.Contains(EPermission.LocationsRead, permissions);
        Assert.Contains(EPermission.LocationsManage, permissions);
    }
    
    [Fact]
    public void MapKeycloakRoleToPermissions_SystemAdmin_ShouldHaveCriticalPermissions()
    {
        // Arrange
        var resolver = CreateResolver();
        var adminRole = "meajudaai-system-admin";

        // Act
        var permissions = resolver.MapKeycloakRoleToPermissions(adminRole).ToList();

        // Assert
        Assert.Contains(EPermission.AdminSystem, permissions);
        Assert.Contains(EPermission.ServiceCatalogsManage, permissions);
    }

    private KeycloakPermissionResolver CreateResolver()
    {
        // Initialize with dummy valid config
        var inMemSettings = new Dictionary<string, string> {
            {"Keycloak:BaseUrl", "http://localhost"},
            {"Keycloak:Realm", "test"},
            {"Keycloak:AdminClientId", "test"},
            {"Keycloak:AdminClientSecret", "test"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemSettings!)
            .Build();

        var httpClient = new HttpClient();
        var logger = Mock.Of<ILogger<KeycloakPermissionResolver>>();
        
        return new KeycloakPermissionResolver(httpClient, configuration, new Mock<HybridCache>().Object, logger);
    }
}
