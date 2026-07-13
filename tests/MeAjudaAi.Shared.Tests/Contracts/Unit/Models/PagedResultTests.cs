using MeAjudaAi.Contracts.Models;

namespace MeAjudaAi.Shared.Tests.Contracts.Unit.Models;

[Trait("Category", "Unit")]
[Trait("Component", "Contracts")]
public class PagedResultTests
{
    [Fact]
    public void Constructor_WithItems_ShouldStoreAllProperties()
    {
        // Arrange
        var items = new[] { "item1", "item2", "item3" };

        // Act
        var result = new PagedResult<string>
        {
            Items = items,
            PageNumber = 2,
            PageSize = 10,
            TotalItems = 50
        };

        // Assert
        result.Items.Should().BeEquivalentTo(items);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalItems.Should().Be(50);
    }

    [Fact]
    public void TotalPages_ShouldCalculateCorrectly()
    {
        // Arrange & Act
        var result100 = new PagedResult<string> { Items = [], PageNumber = 1, PageSize = 10, TotalItems = 100 };
        var result95 = new PagedResult<string> { Items = [], PageNumber = 1, PageSize = 10, TotalItems = 95 };
        var result0 = new PagedResult<string> { Items = [], PageNumber = 1, PageSize = 10, TotalItems = 0 };

        // Assert
        result100.TotalPages.Should().Be(10);
        result95.TotalPages.Should().Be(10);
        result0.TotalPages.Should().Be(0);
    }

    [Fact]
    public void HasPreviousPage_WhenOnFirstPage_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = new PagedResult<string> { Items = [], PageNumber = 1, PageSize = 10, TotalItems = 50 };

        // Assert
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_WhenOnLaterPage_ShouldReturnTrue()
    {
        // Arrange & Act
        var result = new PagedResult<string> { Items = [], PageNumber = 3, PageSize = 10, TotalItems = 50 };

        // Assert
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_WhenOnLastPage_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = new PagedResult<string> { Items = [], PageNumber = 5, PageSize = 10, TotalItems = 50 };

        // Assert
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_WhenNotOnLastPage_ShouldReturnTrue()
    {
        // Arrange & Act
        var result = new PagedResult<string> { Items = [], PageNumber = 2, PageSize = 10, TotalItems = 50 };

        // Assert
        result.HasNextPage.Should().BeTrue();
    }

    [Theory]
    [InlineData(1, 10, 25, false, true)]   // Page 1 of 3
    [InlineData(2, 10, 25, true, true)]    // Page 2 of 3
    [InlineData(3, 10, 25, true, false)]   // Page 3 of 3 (last)
    public void Pagination_ShouldCalculateNavigationCorrectly(
        int pageNumber, int pageSize, int totalItems, bool expectedHasPrevious, bool expectedHasNext)
    {
        // Arrange & Act
        var result = new PagedResult<string>
        {
            Items = [],
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };

        // Assert
        result.HasPreviousPage.Should().Be(expectedHasPrevious);
        result.HasNextPage.Should().Be(expectedHasNext);
    }
}
