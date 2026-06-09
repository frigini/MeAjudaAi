using MeAjudaAi.Modules.Bookings.Application.Authorization;
using MeAjudaAi.Modules.Bookings.Application.Authorization.Models;
using MeAjudaAi.Modules.Bookings.Application.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Authorization;

public class ProviderAuthorizationResultExtensionsTests
{
    [Fact]
    public void ToProblemResult_WhenUpstreamFailure_ReturnsInternalServerError()
    {
        // Arrange
        var result = ProviderAuthorizationResult.UpstreamFailure("Upstream error", StatusCodes.Status502BadGateway);

        // Act
        var response = result.ToProblemResult();

        // Assert
        response.Should().BeOfType<ProblemHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status502BadGateway);
    }

    [Fact]
    public void ToProblemResult_WhenUnauthorized_ReturnsUnauthorized()
    {
        // Arrange
        var result = ProviderAuthorizationResult.Unauthorized("Unauthorized");

        // Act
        var response = result.ToProblemResult();

        // Assert
        response.Should().BeOfType<ProblemHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public void ToProblemResult_WhenNotLinked_ReturnsNotFound()
    {
        // Arrange
        var result = ProviderAuthorizationResult.NotLinked(Guid.NewGuid());

        // Act
        var response = result.ToProblemResult();

        // Assert
        response.Should().BeOfType<ProblemHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public void ToProblemResult_WhenNone_ReturnsNull()
    {
        // Arrange
        var result = ProviderAuthorizationResult.Admin(Guid.NewGuid());

        // Act
        var response = result.ToProblemResult();

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public void ToProblemResult_WhenNoneWithFailureKindNone_ReturnsNull()
    {
        // Arrange
        var result = new ProviderAuthorizationResult { FailureKind = EAuthorizationFailureKind.None };

        // Act
        var response = result.ToProblemResult();

        // Assert
        response.Should().BeNull();
    }
}
