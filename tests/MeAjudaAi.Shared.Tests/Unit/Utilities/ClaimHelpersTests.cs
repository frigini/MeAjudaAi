using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Shared.Utilities;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Utilities;

public class ClaimHelpersTests
{
    [Fact]
    public void GetUserId_ShouldReturnSub_WhenSubClaimExists()
    {
        // Arrange
        var claims = new[] { new Claim("sub", "user-123") };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = ClaimHelpers.GetUserId(principal);

        // Assert
        result.Should().Be("user-123");
    }

    [Fact]
    public void GetUserId_ShouldReturnId_WhenIdClaimExistsAndSubDoesNot()
    {
        // Arrange
        var claims = new[] { new Claim("id", "user-456") };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = ClaimHelpers.GetUserId(principal);

        // Assert
        result.Should().Be("user-456");
    }

    [Fact]
    public void GetUserId_ShouldReturnNameIdentifier_WhenOnlyNameIdentifierExists()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-789") };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = ClaimHelpers.GetUserId(principal);

        // Assert
        result.Should().Be("user-789");
    }

    [Fact]
    public void GetUserId_ShouldReturnNull_WhenNoUserIdClaimsExist()
    {
        // Arrange
        var claims = new[] { new Claim("role", "admin") };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = ClaimHelpers.GetUserId(principal);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUserId_ShouldReturnNull_WhenPrincipalIsNull()
    {
        // Act
        var result = ClaimHelpers.GetUserId(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUserIdGuid_ShouldReturnGuid_WhenValidGuidStringExists()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var claims = new[] { new Claim("sub", guid.ToString()) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = ClaimHelpers.GetUserIdGuid(principal);

        // Assert
        result.Should().Be(guid);
    }

    [Fact]
    public void GetUserIdGuid_ShouldReturnNull_WhenInvalidGuidStringExists()
    {
        // Arrange
        var claims = new[] { new Claim("sub", "not-a-guid") };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = ClaimHelpers.GetUserIdGuid(principal);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUserIdGuid_FromHttpContext_ShouldReturnGuid_WhenValid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var claims = new[] { new Claim("sub", guid.ToString()) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        
        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(c => c.User).Returns(principal);

        // Act
        var result = ClaimHelpers.GetUserIdGuid(contextMock.Object);

        // Assert
        result.Should().Be(guid);
    }

    [Fact]
    public void GetUserIdGuid_FromHttpContext_ShouldThrow_WhenContextIsNull()
    {
        // Act
        var act = () => ClaimHelpers.GetUserIdGuid((HttpContext)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
