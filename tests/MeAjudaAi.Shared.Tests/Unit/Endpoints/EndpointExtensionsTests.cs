using System.Net;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;
using MeAjudaAi.Contracts.Utilities.Constants;

namespace MeAjudaAi.Shared.Tests.Unit.Endpoints;

public class EndpointExtensionsTests
{
    [Fact]
    public void Handle_Success_ShouldReturnOk()
    {
        // Arrange
        var result = Result<string>.Success("test value");

        // Act
        var response = EndpointExtensions.Handle(result);

        // Assert
        response.Should().BeOfType<Ok<Result<string>>>();
        var okResult = (Ok<Result<string>>)response;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.IsSuccess.Should().BeTrue();
        okResult.Value.Value.Should().Be("test value");
    }

    [Fact]
    public void Handle_Success_WithCreatedRoute_ShouldReturnCreated()
    {
        // Arrange
        var result = Result<string>.Success("test value");
        var routeValues = new { id = 1 };

        // Act
        var response = EndpointExtensions.Handle(result, "GetById", routeValues);

        // Assert
        response.Should().BeOfType<CreatedAtRoute<Result<string>>>();
        var createdResult = (CreatedAtRoute<Result<string>>)response;
        createdResult.StatusCode.Should().Be(201);
        createdResult.RouteName.Should().Be("GetById");
        createdResult.RouteValues["id"].Should().Be(1);
        createdResult.Value.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Handle_Failure_NotFound_ShouldReturnNotFound()
    {
        // Arrange
        var error = Error.NotFound(ValidationMessages.NotFound.Resource);
        var result = Result<string>.Failure(error);

        // Act
        var response = EndpointExtensions.Handle(result);

        // Assert
        response.Should().BeOfType<NotFound<Result<string>>>();
        var notFoundResult = (NotFound<Result<string>>)response;
        notFoundResult.StatusCode.Should().Be(404);
        notFoundResult.Value.IsFailure.Should().BeTrue();
        notFoundResult.Value.Error.Should().Be(error);
    }

    [Fact]
    public void Handle_Failure_BadRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var error = Error.BadRequest("Validation error");
        var result = Result<string>.Failure(error);

        // Act
        var response = EndpointExtensions.Handle(result);

        // Assert
        response.Should().BeOfType<BadRequest<Result<string>>>();
        var badRequestResult = (BadRequest<Result<string>>)response;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Handle_NonGeneric_Success_ShouldReturnOk()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var response = EndpointExtensions.Handle(result);

        // Assert
        response.Should().BeOfType<Ok<Result>>();
        var okResult = (Ok<Result>)response;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Handle_NonGeneric_Failure_ShouldReturnError()
    {
        // Arrange
        var error = Error.BadRequest("Conflict error");
        var result = Result.Failure(error);

        // Act
        var response = EndpointExtensions.Handle(result);

        // Assert
        response.Should().BeOfType<BadRequest<Result<object>>>();
    }
    
    [Fact]
    public void Handle_Failure_Unauthorized_ShouldReturnUnauthorized()
    {
         // Arrange
        var error = Error.Unauthorized(ValidationMessages.Generic.Unauthorized);
        var result = Result<string>.Failure(error);

        // Act
        var response = EndpointExtensions.Handle(result);

        // Assert
        response.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public void HandlePaged_Success_ShouldReturnOkWithPagedResponse()
    {
        // Arrange
        var data = new List<string> { "item1", "item2" };
        var result = Result<IEnumerable<string>>.Success(data);
        int totalCount = 10;
        int currentPage = 1;
        int pageSize = 5;

        // Act
        var response = EndpointExtensions.HandlePaged(result, totalCount, currentPage, pageSize);

        // Assert
        response.Should().BeOfType<Ok<Result<PagedResponse<IEnumerable<string>>>>>();
        var okResult = (Ok<Result<PagedResponse<IEnumerable<string>>>>)response;
        okResult.Value.Value.TotalCount.Should().Be(totalCount);
        okResult.Value.Value.Data.Should().BeEquivalentTo(data);
    }
    
    [Fact]
    public void HandlePaged_Failure_ShouldReturnError()
    {
        // Arrange
        var error = Error.BadRequest("Error with page");
        var result = Result<IEnumerable<string>>.Failure(error);

        // Act
        var response = EndpointExtensions.HandlePaged(result, 0, 1, 10);

        // Assert
        response.Should().BeOfType<BadRequest<Result<IEnumerable<string>>>>();
    }

    [Fact]
    public void HandlePagedResult_Success_ShouldReturnOk()
    {
        // Arrange
        var pagedResult = new PagedResult<string> { Items = new List<string> { "item1" }, PageNumber = 1, PageSize = 1, TotalItems = 1 };
        var result = Result<PagedResult<string>>.Success(pagedResult);

        // Act
        var response = EndpointExtensions.HandlePagedResult(result);

        // Assert
        response.Should().BeOfType<Ok<Result<PagedResult<string>>>>();
    }

    [Fact]
    public void HandleNoContent_Success_ShouldReturnNoContent()
    {
        // Arrange
        var result = Result<string>.Success("ignore me");

        // Act
        var response = EndpointExtensions.HandleNoContent(result);

        // Assert
        response.Should().BeOfType<NoContent>();
        var noContentResult = (NoContent)response;
        noContentResult.StatusCode.Should().Be(204);
    }

    [Fact]
    public void HandleNoContent_Failure_ShouldReturnError()
    {
        // Arrange
        var error = Error.BadRequest("Error");
        var result = Result<string>.Failure(error);

        // Act
        var response = EndpointExtensions.HandleNoContent(result);

        // Assert
        response.Should().BeOfType<BadRequest<Result<string>>>();
    }
}
