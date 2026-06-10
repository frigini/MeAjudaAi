using System.Net;
using System.Net.Http.Json;
using MeAjudaAi.Modules.Communications.API.Endpoints.Public;
using MeAjudaAi.E2E.Tests.Base;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Modules.Communications;

public class EmailTemplateEndToEndTests : BaseTestContainerTest
{
    public EmailTemplateEndToEndTests() { }

    [Fact]
    public async Task Post_CreateTemplate_ShouldReturnCreated()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var command = new { 
            Key = "new_template", 
            Subject = "Test", 
            HtmlBody = "<p>Test</p>", 
            TextBody = "Test", 
            IsSystemTemplate = false, 
            Language = "pt-BR",
            CorrelationId = Guid.NewGuid()
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/v1/communications/templates", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Put_UpdateTemplate_ShouldReturnNoContent()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var templateId = Guid.NewGuid();
        var body = new { Subject = "Updated", HtmlBody = "<p>Updated</p>", TextBody = "Updated" };

        // Act
        var response = await ApiClient.PutAsJsonAsync($"/api/v1/communications/templates/{templateId}", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
