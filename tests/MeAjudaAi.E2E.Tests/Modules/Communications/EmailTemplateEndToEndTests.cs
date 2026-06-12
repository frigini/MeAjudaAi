using MeAjudaAi.E2E.Tests.Base;
using System.Net.Http.Json;
using System.Text.Json;

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
            Key = $"template_{Guid.NewGuid():N}",
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
    public async Task Put_UpdateNonExistentTemplate_ShouldReturnNotFound()
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

    [Fact]
    public async Task FullLifecycle_CreateUpdateDeactivateActivateList_ShouldWork()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueKey = $"lifecycle_{Guid.NewGuid():N}";

        // Act 1 - Create template
        var createCommand = new
        {
            Key = uniqueKey,
            Subject = "Original Subject",
            HtmlBody = "<p>Original body</p>",
            TextBody = "Original text",
            IsSystemTemplate = false,
            Language = "pt-BR",
            CorrelationId = Guid.NewGuid()
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/communications/templates", createCommand);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var locationHeader = createResponse.Headers.Location?.ToString();
        locationHeader.Should().NotBeNullOrEmpty();

        var templateId = ExtractIdFromLocation(locationHeader);

        // Act 2 - Update template (creates version 2, deactivates version 1)
        var updateBody = new
        {
            Subject = "Updated Subject",
            HtmlBody = "<p>Updated body</p>",
            TextBody = "Updated text"
        };

        var updateResponse = await ApiClient.PutAsJsonAsync($"/api/v1/communications/templates/{templateId}", updateBody);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act 3 - List templates and verify update (should show version 2)
        var listResponse = await ApiClient.GetAsync("/api/v1/communications/templates");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var templatesJson = await listResponse.Content.ReadAsStringAsync();
        var templates = JsonSerializer.Deserialize<JsonElement[]>(templatesJson, JsonOptions);
        templates.Should().NotBeNull();

        var updatedTemplate = templates!.FirstOrDefault(t =>
            t.GetProperty("key").GetString() == uniqueKey);

        updatedTemplate.ValueKind.Should().NotBe(JsonValueKind.Undefined,
            "Template should be found in list after creation");

        updatedTemplate.GetProperty("subject").GetString().Should().Be("Updated Subject");
        updatedTemplate.GetProperty("htmlBody").GetString().Should().Be("<p>Updated body</p>");
        updatedTemplate.GetProperty("textBody").GetString().Should().Be("Updated text");
        updatedTemplate.GetProperty("version").GetInt32().Should().Be(2,
            "Version should increment after update");

        // Get the new template ID (version 2)
        var newTemplateId = Guid.Parse(updatedTemplate.GetProperty("id").GetString()!);

        // Act 4 - Deactivate the NEW template (version 2)
        var deactivateResponse = await ApiClient.PatchAsync(
            $"/api/v1/communications/templates/{newTemplateId}/deactivate", null);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act 5 - List templates and verify deactivated template is not returned
        var listAfterDeactivate = await ApiClient.GetAsync("/api/v1/communications/templates");
        listAfterDeactivate.StatusCode.Should().Be(HttpStatusCode.OK);

        var templatesAfterDeactivate = await listAfterDeactivate.Content.ReadAsStringAsync();
        var deactivatedCheck = templatesAfterDeactivate.Contains(uniqueKey);
        deactivatedCheck.Should().BeFalse(
            "Deactivated template should not appear in list");

        // Act 6 - Activate template
        var activateResponse = await ApiClient.PatchAsync(
            $"/api/v1/communications/templates/{newTemplateId}/activate", null);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act 7 - List templates and verify activated template is returned
        var listAfterActivate = await ApiClient.GetAsync("/api/v1/communications/templates");
        listAfterActivate.StatusCode.Should().Be(HttpStatusCode.OK);

        var templatesAfterActivate = await listAfterActivate.Content.ReadAsStringAsync();
        var activatedCheck = templatesAfterActivate.Contains(uniqueKey);
        activatedCheck.Should().BeTrue(
            "Activated template should appear in list again");

        // Verify content is still correct after activate/deactivate cycle
        var templatesAfterActivateArray = JsonSerializer.Deserialize<JsonElement[]>(
            templatesAfterActivate, JsonOptions);
        templatesAfterActivateArray.Should().NotBeNull();

        var finalTemplate = templatesAfterActivateArray!.FirstOrDefault(t =>
            t.GetProperty("key").GetString() == uniqueKey);

        finalTemplate.GetProperty("subject").GetString().Should().Be("Updated Subject");
        finalTemplate.GetProperty("version").GetInt32().Should().Be(2,
            "Version should remain the same after activate/deactivate cycle");
    }

    [Fact]
    public async Task Put_UpdateSystemTemplate_ShouldReturnBadRequest()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueKey = $"sys_template_{Guid.NewGuid():N}";
        var createCommand = new
        {
            Key = uniqueKey,
            Subject = "System Template",
            HtmlBody = "...",
            TextBody = "...",
            IsSystemTemplate = true, // System template
            Language = "pt-BR",
            CorrelationId = Guid.NewGuid()
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/communications/templates", createCommand);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var templateId = ExtractIdFromLocation(createResponse.Headers.Location?.ToString());

        // Act - Try to update the system template
        var updateBody = new { Subject = "Updated", HtmlBody = "...", TextBody = "..." };
        var updateResponse = await ApiClient.PutAsJsonAsync($"/api/v1/communications/templates/{templateId}", updateBody);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "System templates are immutable and should return BadRequest on update");
    }

    private new Guid ExtractIdFromLocation(string? location)
    {
        if (location == null) return Guid.Empty;
        var parts = location.Split('/');
        return Guid.Parse(parts[^1]);
    }
}
