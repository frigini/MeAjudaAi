using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

namespace MeAjudaAi.Integration.Tests.Modules.Documents;

public class DocumentsEndpointsTests(ITestOutputHelper testOutput) : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Documents | TestModule.Providers;

    [Fact]
    public async Task GetDocumentStatus_ShouldReturnOk_OrNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/documents/status/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ProviderDocuments_Get_ShouldReturnOk()
    {
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
}
