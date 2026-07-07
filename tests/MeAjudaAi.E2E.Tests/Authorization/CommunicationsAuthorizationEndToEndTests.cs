using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using System.Net.Http.Json;

namespace MeAjudaAi.E2E.Tests.Authorization;

public class CommunicationsAuthorizationEndToEndTests(TestContainerFixture fixture) : IClassFixture<TestContainerFixture>
{
    [Fact]
    public async Task GetCommunicationLogs_WithoutCommunicationsRead_ShouldReturnForbidden()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-123",
            userName: "user",
            email: "user@test.com",
            permissions: []
        );

        // Act
        var response = await fixture.ApiClient.GetAsync("/api/v1/communications/logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetEmailTemplates_WithoutCommunicationsRead_ShouldReturnForbidden()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-123",
            userName: "user",
            email: "user@test.com",
            permissions: []
        );

        // Act
        var response = await fixture.ApiClient.GetAsync("/api/v1/communications/templates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateEmailTemplate_WithoutCommunicationsManage_ShouldReturnForbidden()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-123",
            userName: "user",
            email: "user@test.com",
            permissions: [EPermission.CommunicationsRead.ToString()]
        );

        // Act
        var response = await fixture.ApiClient.PostAsJsonAsync("/api/v1/communications/templates", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateEmailTemplate_WithoutCommunicationsManage_ShouldReturnForbidden()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-123",
            userName: "user",
            email: "user@test.com",
            permissions: [EPermission.CommunicationsRead.ToString()]
        );

        // Act
        var response = await fixture.ApiClient.PutAsJsonAsync($"/api/v1/communications/templates/{Guid.NewGuid()}", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
