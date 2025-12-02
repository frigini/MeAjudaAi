using FluentAssertions;
using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.Shared.Tests.Unit.Contracts;

public class ResponseTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeWithSuccessCode()
    {
        // Act
        var response = new Response<string>();

        // Assert
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeNull();
        response.Message.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithData_ShouldStoreData()
    {
        // Arrange
        var data = "test data";

        // Act
        var response = new Response<string>(data);

        // Assert
        response.Data.Should().Be(data);
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithCustomStatusCode_ShouldStoreStatusCode()
    {
        // Arrange
        var statusCode = 201;

        // Act
        var response = new Response<string>("created", statusCode);

        // Assert
        response.StatusCode.Should().Be(statusCode);
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        var message = "Operation completed successfully";

        // Act
        var response = new Response<string>("data", 200, message);

        // Assert
        response.Message.Should().Be(message);
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(201, true)]
    [InlineData(204, true)]
    [InlineData(299, true)]
    [InlineData(100, false)]
    [InlineData(199, false)]
    [InlineData(300, false)]
    [InlineData(400, false)]
    [InlineData(404, false)]
    [InlineData(500, false)]
    public void IsSuccess_ShouldBeTrueForStatusCode2xx(int statusCode, bool expectedIsSuccess)
    {
        // Act
        var response = new Response<string>(null, statusCode);

        // Assert
        response.IsSuccess.Should().Be(expectedIsSuccess);
    }

    [Fact]
    public void Response_WithComplexData_ShouldWork()
    {
        // Arrange
        var data = new TestModel { Id = 1, Name = "Test" };

        // Act
        var response = new Response<TestModel>(data);

        // Assert
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(1);
        response.Data.Name.Should().Be("Test");
    }

    [Fact]
    public void Response_WithNullData_ShouldAllowNull()
    {
        // Act
        var response = new Response<string?>(null);

        // Assert
        response.Data.Should().BeNull();
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RecordEquality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var response1 = new Response<string>("data", 200, "message");
        var response2 = new Response<string>("data", 200, "message");

        // Act & Assert
        response1.Should().Be(response2);
    }

    [Fact]
    public void RecordEquality_WithDifferentData_ShouldNotBeEqual()
    {
        // Arrange
        var response1 = new Response<string>("data1");
        var response2 = new Response<string>("data2");

        // Act & Assert
        response1.Should().NotBe(response2);
    }

    [Fact]
    public void With_ShouldCreateNewInstanceWithUpdatedMessage()
    {
        // Arrange
        var original = new Response<string>("data", 200, "original message");

        // Act
        var updated = original with { Message = "updated message" };

        // Assert
        updated.Message.Should().Be("updated message");
        updated.Data.Should().Be("data");
        updated.StatusCode.Should().Be(200);
        original.Message.Should().Be("original message"); // Original unchanged
    }

    private class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

public class PagedRequestTests
{
    [Fact]
    public void DefaultConstructor_ShouldHaveDefaultPagination()
    {
        // Act
        var request = new TestPagedRequest();

        // Assert
        request.PageSize.Should().Be(10);
        request.PageNumber.Should().Be(1);
    }

    [Fact]
    public void PageSize_CanBeCustomized()
    {
        // Arrange
        var pageSize = 25;

        // Act
        var request = new TestPagedRequest { PageSize = pageSize };

        // Assert
        request.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public void PageNumber_CanBeCustomized()
    {
        // Arrange
        var pageNumber = 5;

        // Act
        var request = new TestPagedRequest { PageNumber = pageNumber };

        // Assert
        request.PageNumber.Should().Be(pageNumber);
    }

    [Fact]
    public void PagedRequest_ShouldInheritFromRequest()
    {
        // Act
        var request = new TestPagedRequest();

        // Assert
        request.Should().BeAssignableTo<Request>();
    }

    [Fact]
    public void UserId_CanBeSet()
    {
        // Arrange
        var userId = "user123";

        // Act
        var request = new TestPagedRequest { UserId = userId };

        // Assert
        request.UserId.Should().Be(userId);
    }

    [Fact]
    public void RecordEquality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var request1 = new TestPagedRequest { PageSize = 20, PageNumber = 3 };
        var request2 = new TestPagedRequest { PageSize = 20, PageNumber = 3 };

        // Act & Assert
        request1.Should().Be(request2);
    }

    [Fact]
    public void With_ShouldCreateNewInstanceWithUpdatedPageSize()
    {
        // Arrange
        var original = new TestPagedRequest { PageSize = 10, PageNumber = 1 };

        // Act
        var updated = original with { PageSize = 50 };

        // Assert
        updated.PageSize.Should().Be(50);
        updated.PageNumber.Should().Be(1);
        original.PageSize.Should().Be(10); // Original unchanged
    }

    private record TestPagedRequest : PagedRequest;
}

public class RequestTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitialize()
    {
        // Act
        var request = new TestRequest();

        // Assert
        request.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_CanBeSet()
    {
        // Arrange
        var userId = "user456";

        // Act
        var request = new TestRequest { UserId = userId };

        // Assert
        request.UserId.Should().Be(userId);
    }

    [Fact]
    public void RecordEquality_WithSameUserId_ShouldBeEqual()
    {
        // Arrange
        var request1 = new TestRequest { UserId = "user789" };
        var request2 = new TestRequest { UserId = "user789" };

        // Act & Assert
        request1.Should().Be(request2);
    }

    [Fact]
    public void RecordEquality_WithDifferentUserId_ShouldNotBeEqual()
    {
        // Arrange
        var request1 = new TestRequest { UserId = "user1" };
        var request2 = new TestRequest { UserId = "user2" };

        // Act & Assert
        request1.Should().NotBe(request2);
    }

    private record TestRequest : Request;
}
