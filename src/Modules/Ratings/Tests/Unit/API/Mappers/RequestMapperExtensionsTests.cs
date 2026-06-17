using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.API.Mappers;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.API.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "Ratings")]
[Trait("Layer", "API")]
public class RequestMapperExtensionsTests
{
    [Fact]
    public void ToCommand_CreateReviewRequest_ShouldMapAllProperties()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new CreateReviewRequest(
            ProviderId: Guid.NewGuid(),
            Rating: 5,
            Comment: "Excellent service!");

        // Act
        var command = request.ToCommand(customerId);

        // Assert
        command.Should().NotBeNull();
        command.ProviderId.Should().Be(request.ProviderId);
        command.CustomerId.Should().Be(customerId);
        command.Rating.Should().Be(5);
        command.Comment.Should().Be("Excellent service!");
    }

    [Fact]
    public void ToCommand_CreateReviewRequest_WithNullComment_ShouldMapNull()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new CreateReviewRequest(
            ProviderId: Guid.NewGuid(),
            Rating: 4,
            Comment: null);

        // Act
        var command = request.ToCommand(customerId);

        // Assert
        command.Comment.Should().BeNull();
    }

    [Fact]
    public void ToCommand_CreateReviewRequest_ShouldMapCustomerIdFromParameter()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new CreateReviewRequest(
            ProviderId: Guid.NewGuid(),
            Rating: 3,
            Comment: null);

        // Act
        var command = request.ToCommand(customerId);

        // Assert
        command.CustomerId.Should().Be(customerId);
    }
}
