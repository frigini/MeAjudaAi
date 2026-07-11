using MeAjudaAi.Contracts.Models;

namespace MeAjudaAi.Shared.Tests.Contracts.Unit.Models;

[Trait("Category", "Unit")]
[Trait("Component", "Contracts")]
public class PagedResponseTests
{
    [Fact]
    public void Constructor_WithData_ShouldStoreAllProperties()
    {
        // Arrange
        var data = new[] { "item1", "item2" };

        // Act
        var response = new PagedResponse<string[]>(data, totalCount: 50, currentPage: 2, pageSize: 10);

        // Assert
        response.Data.Should().BeEquivalentTo(data);
        response.TotalCount.Should().Be(50);
        response.CurrentPage.Should().Be(2);
        response.PageSize.Should().Be(10);
    }

    [Theory]
    [InlineData(100, 10, 10)]
    [InlineData(95, 10, 10)]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 1)]
    [InlineData(11, 10, 2)]
    public void TotalPages_ShouldCalculateCorrectly(int totalCount, int pageSize, int expectedPages)
    {
        // Arrange & Act
        var response = new PagedResponse<string>(null, totalCount: totalCount, currentPage: 1, pageSize: pageSize);

        // Assert
        response.TotalPages.Should().Be(expectedPages);
    }

    [Fact]
    public void Constructor_WithNullData_ShouldAllowNull()
    {
        // Act
        var response = new PagedResponse<string>(null, totalCount: 0, currentPage: 1, pageSize: 10);

        // Assert
        response.Data.Should().BeNull();
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    public void RecordEquality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var response1 = new PagedResponse<string>("data", 10, 1, 10);
        var response2 = new PagedResponse<string>("data", 10, 1, 10);

        // Act & Assert
        response1.Should().Be(response2);
    }

    [Fact]
    public void With_ShouldCreateNewInstanceWithUpdatedPage()
    {
        // Arrange
        var original = new PagedResponse<string>("data", 100, 1, 10);

        // Act
        var updated = original with { CurrentPage = 5 };

        // Assert
        updated.CurrentPage.Should().Be(5);
        updated.TotalCount.Should().Be(100);
        original.CurrentPage.Should().Be(1);
    }
}
