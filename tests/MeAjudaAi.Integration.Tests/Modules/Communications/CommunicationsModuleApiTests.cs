using MeAjudaAi.Integration.Tests.Base;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Contracts.Models;

namespace MeAjudaAi.Integration.Tests.Modules.Communications;

public class CommunicationsModuleApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Communications;

    [Fact]
    public async Task GetLogs_WithAuthentication_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/communications/logs?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CommunicationLogDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty(); // Inicialmente vazio
    }

    [Fact]
    public async Task GetTemplates_WithAuthentication_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/communications/templates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<EmailTemplateDto>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLogs_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/communications/logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTemplates_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/communications/templates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
