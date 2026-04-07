using FluentAssertions;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Extensions;

public class HttpContextExtensionsTests
{
    [Fact]
    public void GetAuditIdentity_WithNullAccessor_ShouldReturnSystem()
    {
        // Arrange
        IHttpContextAccessor? accessor = null;

        // Act
        var result = accessor.GetAuditIdentity();

        // Assert
        result.Should().Be("system");
    }

    [Fact]
    public void GetAuditIdentity_WithNullHttpContext_ShouldReturnSystem()
    {
        // Arrange
        var accessor = new HttpContextAccessor { HttpContext = null };

        // Act
        var result = accessor.GetAuditIdentity();

        // Assert
        result.Should().Be("system");
    }

    [Fact]
    public void GetAuditIdentity_WithUserWithEmail_ShouldReturnEmail()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Email, "test@example.com") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        
        var context = new DefaultHttpContext { User = principal };
        var accessor = new HttpContextAccessor { HttpContext = context };

        // Act
        var result = accessor.GetAuditIdentity();

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void GetAuditIdentity_WithUserWithPreferredUsername_ShouldReturnPreferredUsername()
    {
        // Arrange
        var claims = new[] 
        { 
            new Claim(AuthConstants.Claims.PreferredUsername, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        
        var context = new DefaultHttpContext { User = principal };
        var accessor = new HttpContextAccessor { HttpContext = context };

        // Act
        var result = accessor.GetAuditIdentity();

        // Assert
        result.Should().Be("testuser");
    }

    [Fact]
    public void GetAuditIdentity_WithUserWithNameIdentifier_ShouldReturnNameIdentifier()
    {
        // Arrange
        var claims = new[] 
        { 
            new Claim(ClaimTypes.NameIdentifier, "user-123")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        
        var context = new DefaultHttpContext { User = principal };
        var accessor = new HttpContextAccessor { HttpContext = context };

        // Act
        var result = accessor.GetAuditIdentity();

        // Assert
        result.Should().Be("user-123");
    }

    [Fact]
    public void GetAuditIdentity_WithUserWithNoClaims_ShouldReturnSystem()
    {
        // Arrange
        var identity = new ClaimsIdentity(Array.Empty<Claim>(), "Test");
        var principal = new ClaimsPrincipal(identity);
        
        var context = new DefaultHttpContext { User = principal };
        var accessor = new HttpContextAccessor { HttpContext = context };

        // Act
        var result = accessor.GetAuditIdentity();

        // Assert
        result.Should().Be("system");
    }

    [Fact]
    public void GetAuditIdentity_WithUserWithMultipleIdentities_ShouldCheckAll()
    {
        // Arrange
        var claims1 = new[] { new Claim(ClaimTypes.Email, "email@test.com") };
        var claims2 = new[] { new Claim(ClaimTypes.NameIdentifier, "user-id") };
        
        var identity1 = new ClaimsIdentity(claims1, "Test1");
        var identity2 = new ClaimsIdentity(claims2, "Test2");
        
        var principal = new ClaimsPrincipal(new[] { identity1, identity2 });
        
        var context = new DefaultHttpContext { User = principal };
        var accessor = new HttpContextAccessor { HttpContext = context };

        // Act
        var result = accessor.GetAuditIdentity();

        // Assert - should return the first valid one (email)
        result.Should().Be("email@test.com");
    }

    [Fact]
    public void GetAuditIdentity_WithEmptyEmailClaim_ShouldFallbackToNext()
    {
        // Arrange
        var claims = new[] 
        { 
            new Claim(ClaimTypes.Email, ""),
            new Claim(AuthConstants.Claims.PreferredUsername, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        
        var context = new DefaultHttpContext { User = principal };
        var accessor = new HttpContextAccessor { HttpContext = context };

        // Act
        var result = accessor.GetAuditIdentity();

        // Assert - should fallback to PreferredUsername
        result.Should().Be("testuser");
    }
}
