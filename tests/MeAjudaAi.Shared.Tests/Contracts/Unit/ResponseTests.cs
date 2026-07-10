using MeAjudaAi.Contracts.Models;

namespace MeAjudaAi.Shared.Tests.Contracts.Unit;

[Trait("Category", "Unit")]
[Trait("Component", "Contracts")]
public class ResponseTests
{
    #region Constructor Tests

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

    #endregion

    #region IsSuccess Tests

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

    #endregion

    #region Data Handling Tests

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

    #endregion

    #region Equality Tests

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
        original.Message.Should().Be("original message");
    }

    #endregion

    private class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}