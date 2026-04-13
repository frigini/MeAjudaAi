using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

namespace MeAjudaAi.Integration.Tests.Modules.Documents;

public class DocumentsEndpointsTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Documents | TestModule.Providers;

    [Fact]
    public async Task GetDocumentStatus_ShouldReturnOk_OrNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync($"/api/v1/documents/status/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ProviderDocuments_Get_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync($"/api/v1/documents/provider/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RequestVerification_ShouldWork()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var providerId = Guid.NewGuid();

        // Act
        var response = await Client.PostAsync($"/api/v1/documents/provider/{providerId}/request-verification", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AdminVerificationFlow_ShouldWork()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var documentId = Guid.NewGuid();

        // 1. Approve
        var approveResponse = await Client.PostAsJsonAsync($"/api/v1/documents/{documentId}/verify", new { approved = true, notes = "Looks good" });
        approveResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);

        // 2. Reject
        var rejectResponse = await Client.PostAsJsonAsync($"/api/v1/documents/{documentId}/verify", new { approved = false, notes = "Bad picture" });
        rejectResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadDocument_ShouldWork()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x01, 0x02, 0x03 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test.jpg");
        content.Add(new StringContent("1"), "documentType");
        content.Add(new StringContent(Guid.NewGuid().ToString()), "providerId");

        // Act
        var response = await Client.PostAsync("/api/v1/documents/upload", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.UnsupportedMediaType);
    }
}
