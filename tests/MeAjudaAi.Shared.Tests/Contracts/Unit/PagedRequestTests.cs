namespace MeAjudaAi.Shared.Tests.Contracts.Unit;

[Trait("Category", "Unit")]
[Trait("Component", "Contracts")]
public class PagedRequestTests
{
    #region Constructor Tests

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

    #endregion

    #region Inheritance Tests

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

    #endregion

    #region Equality Tests

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
        original.PageSize.Should().Be(10);
    }

    #endregion

    public abstract record Request
    {
        public string? UserId { get; init; }
    }

    public record TestPagedRequest : Request
    {
        public int PageSize { get; init; } = 10;
        public int PageNumber { get; init; } = 1;
    }
}
