using MeAjudaAi.Contracts.Models;
using System.Text.Json;

namespace MeAjudaAi.Shared.Tests.Contracts.Unit;

[Trait("Category", "Unit")]
[Trait("Component", "Contracts")]
public class ApiErrorResponseTests
{
    [Fact]
    public void DefaultValues_ShouldHaveUtcNow()
    {
        // Arrange & Act
        var error = new ApiErrorResponse();

        // Assert
        error.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        error.Title.Should().BeEmpty();
        error.Detail.Should().BeEmpty();
        error.TraceId.Should().BeNull();
        error.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange & Act
        var error = new ApiErrorResponse
        {
            StatusCode = 400,
            Title = "Bad Request",
            Detail = "Invalid input data",
            TraceId = "abc123",
            Timestamp = new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc),
            ValidationErrors = new Dictionary<string, string[]>
            {
                ["Name"] = ["Required", "Max length 100"],
                ["Email"] = ["Invalid format"]
            }
        };

        // Assert
        error.StatusCode.Should().Be(400);
        error.Title.Should().Be("Bad Request");
        error.Detail.Should().Be("Invalid input data");
        error.TraceId.Should().Be("abc123");
        error.ValidationErrors.Should().HaveCount(2);
        error.ValidationErrors!["Name"].Should().HaveCount(2);
    }

    [Fact]
    public void ShouldSerializeAndDeserialize()
    {
        // Arrange
        var error = new ApiErrorResponse
        {
            StatusCode = 422,
            Title = "Validation Error",
            Detail = "One or more validation errors occurred",
            ValidationErrors = new Dictionary<string, string[]> { ["Field"] = ["Required"] }
        };

        // Act
        var json = JsonSerializer.Serialize(error);
        var deserialized = JsonSerializer.Deserialize<ApiErrorResponse>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.StatusCode.Should().Be(422);
        deserialized.Title.Should().Be("Validation Error");
        deserialized.ValidationErrors.Should().ContainKey("Field");
    }
}
