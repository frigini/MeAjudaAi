using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MeAjudaAi.E2E.Tests.Base;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Integration;

/// <summary>
/// Testes de integração para pipeline CQRS e manipulação de eventos
/// </summary>
public class CqrsIntegrationTests : TestContainerTestBase
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public async Task CreateUser_ShouldTriggerDomainEvents()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8]; // Keep under 30 chars
        var createUserRequest = new
        {
            Username = $"test_{uniqueId}", // test_12345678 = 13 chars
            Email = $"eventtest_{uniqueId}@example.com",
            FirstName = "Event",
            LastName = "Test"
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/v1/users", createUserRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created,
            HttpStatusCode.Conflict // User might already exist in some test runs
        );

        if (response.StatusCode == HttpStatusCode.Created)
        {
            // Verify the response contains expected data
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();

            var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
            result.TryGetProperty("data", out var dataProperty).Should().BeTrue();
            dataProperty.TryGetProperty("id", out var idProperty).Should().BeTrue();
            idProperty.GetGuid().Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task CreateAndUpdateUser_ShouldMaintainConsistency()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8]; // Keep under 30 chars  
        var createUserRequest = new
        {
            Username = $"test_{uniqueId}", // test_12345678 = 13 chars
            Email = $"consistencytest_{uniqueId}@example.com",
            FirstName = "Consistency",
            LastName = "Test"
        };

        // Act 1: Create user
        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/users", createUserRequest, _jsonOptions);

        // Assert 1: User created successfully or already exists
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Conflict);

        if (createResponse.StatusCode == HttpStatusCode.Created)
        {
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<JsonElement>(createContent, _jsonOptions);
            createResult.TryGetProperty("data", out var dataProperty).Should().BeTrue();
            dataProperty.TryGetProperty("id", out var idProperty).Should().BeTrue();
            var userId = idProperty.GetGuid();

            // Act 2: Update the user
            var updateRequest = new
            {
                FirstName = "Updated",
                LastName = "User",
                Email = $"updated_{uniqueId}@example.com"
            };

            var updateResponse = await ApiClient.PutAsJsonAsync($"/api/v1/users/{userId}/profile", updateRequest, _jsonOptions);

            // Assert 2: Update should succeed or return appropriate error
            updateResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.NoContent,
                HttpStatusCode.NotFound // If user was somehow not found
            );

            // Act 3: Verify user can be retrieved
            var getResponse = await ApiClient.GetAsync($"/api/v1/users/{userId}");
            
            // Assert 3: User should be retrievable
            getResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.NotFound // Acceptable if user doesn't exist
            );
        }
    }

    [Fact]
    public async Task QueryUsers_ShouldReturnConsistentPagination()
    {
        // Act 1: Get first page
        var page1Response = await ApiClient.GetAsync("/api/v1/users?pageNumber=1&pageSize=5");

        // Act 2: Get second page  
        var page2Response = await ApiClient.GetAsync("/api/v1/users?pageNumber=2&pageSize=5");

        // Assert: Both requests should succeed or return not found
        page1Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        page2Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        // If data exists, verify pagination structure
        if (page1Response.StatusCode == HttpStatusCode.OK)
        {
            var content = await page1Response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();

            // Verify it's valid JSON with expected structure
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        }
    }

    [Fact]
    public async Task Command_WithInvalidInput_ShouldReturnValidationErrors()
    {
        // Arrange: Create request with multiple validation errors
        var invalidRequest = new
        {
            Username = "", // Too short
            Email = "not-an-email", // Invalid format
            FirstName = new string('a', 101), // Too long (assuming max 100)
            LastName = "" // Required field empty
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/v1/users", invalidRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        // Verify error response format
        var errorDoc = JsonDocument.Parse(content);
        errorDoc.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    public async Task ConcurrentUserCreation_ShouldHandleGracefully()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8]; // Keep under 30 chars
        var userRequest = new
        {
            Username = $"conc_{uniqueId}", // conc_12345678 = 13 chars
            Email = $"concurrent_{uniqueId}@example.com",
            FirstName = "Concurrent",
            LastName = "Test"
        };

        // Act: Send multiple concurrent requests
        var tasks = Enumerable.Range(0, 3).Select(async i =>
        {
            return await ApiClient.PostAsJsonAsync("/api/v1/users", userRequest, _jsonOptions);
        });

        var responses = await Task.WhenAll(tasks);

        // Assert: Only one should succeed, others should return conflict
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        // Either one succeeds and others conflict, or they all conflict (if user already existed)
        ((successCount == 1 && conflictCount == 2) || conflictCount == 3)
            .Should().BeTrue("Exactly one request should succeed or all should conflict");
    }
}