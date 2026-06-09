using MeAjudaAi.Modules.Bookings.API.Extensions;
using MeAjudaAi.Modules.Bookings.Application.Common;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.API.Extensions;

public class ProviderAuthorizationResultExtensionsTests
{
    [Fact]
    public void ToProblemResult_WhenUpstreamFailure_ReturnsInternalServerError()
    {
        var result = ProviderAuthorizationResult.UpstreamFailure("Upstream error", StatusCodes.Status502BadGateway);

        var response = result.ToProblemResult();

        response.Should().NotBeNull();
    }

    [Fact]
    public void ToProblemResult_WhenUnauthorized_ReturnsUnauthorized()
    {
        var result = ProviderAuthorizationResult.Unauthorized("Unauthorized");

        var response = result.ToProblemResult();

        response.Should().NotBeNull();
    }

    [Fact]
    public void ToProblemResult_WhenNotLinked_ReturnsNotFound()
    {
        var result = ProviderAuthorizationResult.NotLinked(Guid.NewGuid());

        var response = result.ToProblemResult();

        response.Should().NotBeNull();
    }

    [Fact]
    public void ToProblemResult_WhenNone_ReturnsNull()
    {
        var result = ProviderAuthorizationResult.Admin(Guid.NewGuid());

        var response = result.ToProblemResult();

        response.Should().BeNull();
    }

    [Fact]
    public void ToProblemResult_WhenNoneWithFailureKindNone_ThrowsNotImplementedException()
    {
        var result = new ProviderAuthorizationResult { FailureKind = AuthorizationFailureKind.None };

        Action act = () => result.ToProblemResult();

        act.Should().Throw<NotImplementedException>();
    }
}
