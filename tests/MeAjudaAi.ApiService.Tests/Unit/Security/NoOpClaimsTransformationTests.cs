using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.ApiService.Extensions;

namespace MeAjudaAi.ApiService.Tests.Unit.Security;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class NoOpClaimsTransformationTests
{
    [Fact]
    public async Task TransformAsync_ShouldReturnSamePrincipal()
    {
        // Arrange
        var transformation = new NoOpClaimsTransformation();
        var claims = new[] { new Claim(ClaimTypes.Name, "test-user") };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await transformation.TransformAsync(principal);

        // Assert
        result.Should().BeSameAs(principal);
    }

    [Fact]
    public async Task TransformAsync_ShouldNotModifyPrincipal()
    {
        // Arrange
        var transformation = new NoOpClaimsTransformation();
        var originalClaims = new[]
        {
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(originalClaims, "test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await transformation.TransformAsync(principal);

        // Assert
        result.Claims.Should().HaveCount(3);
        result.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "test-user");
        result.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
        result.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public async Task TransformAsync_WithEmptyPrincipal_ShouldReturnSamePrincipal()
    {
        // Arrange
        var transformation = new NoOpClaimsTransformation();
        var principal = new ClaimsPrincipal();

        // Act
        var result = await transformation.TransformAsync(principal);

        // Assert
        result.Should().BeSameAs(principal);
        result.Claims.Should().BeEmpty();
    }
}
