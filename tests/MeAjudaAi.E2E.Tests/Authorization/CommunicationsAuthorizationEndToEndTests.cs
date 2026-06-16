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
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-123",
            userName: "user",
            email: "user@test.com",
            permissions: [] // SEM permissão CommunicationsRead
        );

        var response = await fixture.ApiClient.GetAsync("/api/v1/communications/logs");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetEmailTemplates_WithoutCommunicationsRead_ShouldReturnForbidden()
    {
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-123",
            userName: "user",
            email: "user@test.com",
            permissions: [] // SEM permissão CommunicationsRead
        );

        var response = await fixture.ApiClient.GetAsync("/api/v1/communications/templates");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateEmailTemplate_WithoutCommunicationsManage_ShouldReturnForbidden()
    {
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-123",
            userName: "user",
            email: "user@test.com",
            permissions: [EPermission.CommunicationsRead.ToString()] // Tem Read mas NÃO Manage
        );

        var response = await fixture.ApiClient.PostAsJsonAsync("/api/v1/communications/templates", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateEmailTemplate_WithoutCommunicationsManage_ShouldReturnForbidden()
    {
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-123",
            userName: "user",
            email: "user@test.com",
            permissions: [EPermission.CommunicationsRead.ToString()] // Tem Read mas NÃO Manage
        );

        var response = await fixture.ApiClient.PutAsJsonAsync($"/api/v1/communications/templates/{Guid.NewGuid()}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
