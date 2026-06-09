using MeAjudaAi.Modules.Bookings.API.Extensions;
using MeAjudaAi.Modules.Bookings.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.API.Extensions;

public class ProviderAuthorizationResultExtensionsTests
{
    [Fact]
    public void ToProblemResult_WhenUpstreamFailure_ReturnsInternalServerError()
    {
        var result = ProviderAuthorizationResult.UpstreamFailure("Upstream error", StatusCodes.Status502BadGateway);

        var response = result.ToProblemResult();

        response.Should().BeOfType<ProblemHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status502BadGateway);
    }

    [Fact]
    public void ToProblemResult_WhenUnauthorized_ReturnsUnauthorized()
    {
        var result = ProviderAuthorizationResult.Unauthorized("Unauthorized");

        var response = result.ToProblemResult();

        response.Should().BeOfType<ProblemHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public void ToProblemResult_WhenNotLinked_ReturnsNotFound()
    {
        var result = ProviderAuthorizationResult.NotLinked(Guid.NewGuid());

        var response = result.ToProblemResult();

        response.Should().BeOfType<ProblemHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public void ToProblemResult_WhenNone_ReturnsNull()
    {
        var result = ProviderAuthorizationResult.Admin(Guid.NewGuid());

        var response = result.ToProblemResult();

        response.Should().BeNull();
    }

    [Fact]
    public void ToProblemResult_WhenNoneWithFailureKindNone_ReturnsNull()
    {
        var result = new ProviderAuthorizationResult { FailureKind = AuthorizationFailureKind.None };

        var response = result.ToProblemResult();

        response.Should().BeNull();
    }
}
