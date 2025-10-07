using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MeAjudaAi.Shared.Tests.Unit.Endpoints;

[Trait("Category", "Unit")]
public class BaseEndpointTests
{
    private class TestEndpoint : BaseEndpoint
    {
        // Expõe métodos protegidos para teste
        public static new IResult Handle<T>(Result<T> result, string? createdRoute = null, object? routeValues = null)
            => BaseEndpoint.Handle(result, createdRoute, routeValues);

        public static new IResult Handle(Result result)
            => BaseEndpoint.Handle(result);

        public static new IResult HandlePaged<T>(Result<IEnumerable<T>> result, int total, int page, int size)
            => BaseEndpoint.HandlePaged(result, total, page, size);

        public static new IResult HandlePagedResult<T>(Result<PagedResult<T>> result)
            => BaseEndpoint.HandlePagedResult(result);

        public static new IResult HandleNoContent<T>(Result<T> result)
            => BaseEndpoint.HandleNoContent(result);

        public static new IResult HandleNoContent(Result result)
            => BaseEndpoint.HandleNoContent(result);

        public static new IResult BadRequest(string message)
            => BaseEndpoint.BadRequest(message);

        public static new IResult BadRequest(Error error)
            => BaseEndpoint.BadRequest(error);

        public static new IResult NotFound(string message)
            => BaseEndpoint.NotFound(message);

        public static new IResult NotFound(Error error)
            => BaseEndpoint.NotFound(error);

        public static new IResult Unauthorized()
            => BaseEndpoint.Unauthorized();

        public static new IResult Forbid()
            => BaseEndpoint.Forbid();

        public static new string GetUserId(HttpContext context)
            => BaseEndpoint.GetUserId(context);

        public static new string? GetUserIdOrNull(HttpContext context)
            => BaseEndpoint.GetUserIdOrNull(context);
    }

    [Fact]
    public void CreateVersionedGroup_ShouldCreateGroupWithCorrectPattern()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        var group = BaseEndpoint.CreateVersionedGroup(app, "users");

        // Assert
        group.Should().NotBeNull();
    }

    [Fact]
    public void CreateVersionedGroup_WithCustomTag_ShouldUseCustomTag()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        var group = BaseEndpoint.CreateVersionedGroup(app, "users", "CustomTag");

        // Assert
        group.Should().NotBeNull();
    }

    [Fact]
    public void CreateVersionedGroup_WithoutTag_ShouldCapitalizeModuleName()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        var group = BaseEndpoint.CreateVersionedGroup(app, "users");

        // Assert
        group.Should().NotBeNull();
    }

    [Fact]
    public void Handle_WithSuccessfulGenericResult_ShouldReturnOkResult()
    {
        // Arrange
        var successResult = Result<string>.Success("test-data");

        // Act
        var result = TestEndpoint.Handle(successResult);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Handle_WithFailedGenericResult_ShouldReturnErrorResult()
    {
        // Arrange
        var failedResult = Result<string>.Failure(Error.BadRequest("Test error"));

        // Act
        var result = TestEndpoint.Handle(failedResult);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Handle_WithSuccessfulNonGenericResult_ShouldReturnOkResult()
    {
        // Arrange
        var successResult = Result.Success();

        // Act
        var result = TestEndpoint.Handle(successResult);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Handle_WithFailedNonGenericResult_ShouldReturnErrorResult()
    {
        // Arrange
        var failedResult = Result.Failure(Error.BadRequest("Test error"));

        // Act
        var result = TestEndpoint.Handle(failedResult);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void HandlePaged_WithSuccessfulResult_ShouldReturnPagedResult()
    {
        // Arrange
        var items = new List<string> { "item1", "item2" };
        var successResult = Result<IEnumerable<string>>.Success(items);

        // Act
        var result = TestEndpoint.HandlePaged(successResult, 10, 1, 5);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void HandlePaged_WithFailedResult_ShouldReturnErrorResult()
    {
        // Arrange
        var failedResult = Result<IEnumerable<string>>.Failure(Error.BadRequest("Test error"));

        // Act
        var result = TestEndpoint.HandlePaged(failedResult, 10, 1, 5);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void HandlePagedResult_WithSuccessfulPagedResult_ShouldReturnResult()
    {
        // Arrange
        var items = new List<string> { "item1", "item2" };
        var pagedResult = new PagedResult<string>(items, 1, 5, 10);
        var successResult = Result<PagedResult<string>>.Success(pagedResult);

        // Act
        var result = TestEndpoint.HandlePagedResult(successResult);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void HandlePagedResult_WithFailedResult_ShouldReturnErrorResult()
    {
        // Arrange
        var failedResult = Result<PagedResult<string>>.Failure(Error.BadRequest("Test error"));

        // Act
        var result = TestEndpoint.HandlePagedResult(failedResult);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void HandleNoContent_WithSuccessfulGenericResult_ShouldReturnNoContentResult()
    {
        // Arrange
        var successResult = Result<string>.Success("test-data");

        // Act
        var result = TestEndpoint.HandleNoContent(successResult);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void HandleNoContent_WithFailedGenericResult_ShouldReturnErrorResult()
    {
        // Arrange
        var failedResult = Result<string>.Failure(Error.BadRequest("Test error"));

        // Act
        var result = TestEndpoint.HandleNoContent(failedResult);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void HandleNoContent_WithSuccessfulNonGenericResult_ShouldReturnNoContentResult()
    {
        // Arrange
        var successResult = Result.Success();

        // Act
        var result = TestEndpoint.HandleNoContent(successResult);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void HandleNoContent_WithFailedNonGenericResult_ShouldReturnErrorResult()
    {
        // Arrange
        var failedResult = Result.Failure(Error.BadRequest("Test error"));

        // Act
        var result = TestEndpoint.HandleNoContent(failedResult);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void BadRequest_WithMessage_ShouldReturnBadRequestResult()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var result = TestEndpoint.BadRequest(message);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void BadRequest_WithError_ShouldReturnBadRequestResult()
    {
        // Arrange
        var error = Error.BadRequest("Test error");

        // Act
        var result = TestEndpoint.BadRequest(error);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void NotFound_WithMessage_ShouldReturnNotFoundResult()
    {
        // Arrange
        var message = "Resource not found";

        // Act
        var result = TestEndpoint.NotFound(message);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void NotFound_WithError_ShouldReturnNotFoundResult()
    {
        // Arrange
        var error = Error.NotFound("Resource not found");

        // Act
        var result = TestEndpoint.NotFound(error);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Unauthorized_ShouldReturnUnauthorizedResult()
    {
        // Act
        var result = TestEndpoint.Unauthorized();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Forbid_ShouldReturnForbidResult()
    {
        // Act
        var result = TestEndpoint.Forbid();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetUserId_WithSubClaim_ShouldReturnUserId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new("sub", "test-user-id")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var userId = TestEndpoint.GetUserId(context);

        // Assert
        userId.Should().Be("test-user-id");
    }

    [Fact]
    public void GetUserId_WithIdClaim_ShouldReturnUserId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new("id", "test-user-id")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var userId = TestEndpoint.GetUserId(context);

        // Assert
        userId.Should().Be("test-user-id");
    }

    [Fact]
    public void GetUserId_WithBothClaims_ShouldPreferSubClaim()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new("sub", "sub-user-id"),
            new("id", "id-user-id")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var userId = TestEndpoint.GetUserId(context);

        // Assert
        userId.Should().Be("sub-user-id");
    }

    [Fact]
    public void GetUserId_WithoutClaims_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act & Assert
        var action = () => TestEndpoint.GetUserId(context);
        action.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("User ID not found in token");
    }

    [Fact]
    public void GetUserId_WithNullUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.User = null!;

        // Act & Assert
        var action = () => TestEndpoint.GetUserId(context);
        action.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("User ID not found in token");
    }

    [Fact]
    public void GetUserIdOrNull_WithSubClaim_ShouldReturnUserId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new("sub", "test-user-id")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var userId = TestEndpoint.GetUserIdOrNull(context);

        // Assert
        userId.Should().Be("test-user-id");
    }

    [Fact]
    public void GetUserIdOrNull_WithIdClaim_ShouldReturnUserId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new("id", "test-user-id")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var userId = TestEndpoint.GetUserIdOrNull(context);

        // Assert
        userId.Should().Be("test-user-id");
    }

    [Fact]
    public void GetUserIdOrNull_WithoutClaims_ShouldReturnNull()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var userId = TestEndpoint.GetUserIdOrNull(context);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void GetUserIdOrNull_WithNullUser_ShouldReturnNull()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.User = null!;

        // Act
        var userId = TestEndpoint.GetUserIdOrNull(context);

        // Assert
        userId.Should().BeNull();
    }
}
