using FluentAssertions;
using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.Shared.Tests.Unit.Contracts;

public class PagedResultTests
{
    [Fact]
    public void Create_ShouldCalculatePaginationProperties()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };
        var page = 2;
        var pageSize = 10;
        var totalCount = 25;

        // Act
        var result = PagedResult<string>.Create(items, page, pageSize, totalCount);

        // Assert
        result.Items.Should().BeEquivalentTo(items);
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.TotalCount.Should().Be(totalCount);
        result.TotalPages.Should().Be(3); // Math.Ceiling(25/10) = 3
        result.HasNextPage.Should().BeTrue(); // page 2 < 3 pages
        result.HasPreviousPage.Should().BeTrue(); // page 2 > 1
    }

    [Fact]
    public void Constructor_WithExplicitValues_ShouldStoreAllProperties()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var page = 1;
        var pageSize = 3;
        var totalCount = 10;
        var totalPages = 4;
        var hasNextPage = true;
        var hasPreviousPage = false;

        // Act
        var result = new PagedResult<int>(items, page, pageSize, totalCount, totalPages, hasNextPage, hasPreviousPage);

        // Assert
        result.Items.Should().BeEquivalentTo(items);
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.TotalCount.Should().Be(totalCount);
        result.TotalPages.Should().Be(totalPages);
        result.HasNextPage.Should().Be(hasNextPage);
        result.HasPreviousPage.Should().Be(hasPreviousPage);
    }

    [Fact]
    public void Constructor_FirstPage_ShouldNotHavePreviousPage()
    {
        // Arrange
        var items = new List<string> { "a", "b" };

        // Act
        var result = new PagedResult<string>(items, page: 1, pageSize: 10, totalCount: 50);

        // Assert
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void Constructor_LastPage_ShouldNotHaveNextPage()
    {
        // Arrange
        var items = new List<string> { "a", "b" };
        var page = 5;
        var pageSize = 10;
        var totalCount = 50;

        // Act
        var result = new PagedResult<string>(items, page, pageSize, totalCount);

        // Assert
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeTrue();
        result.TotalPages.Should().Be(5);
    }

    [Fact]
    public void Constructor_OnlyOnePage_ShouldHaveNoNavigation()
    {
        // Arrange
        var items = new List<string> { "a", "b", "c" };

        // Act
        var result = new PagedResult<string>(items, page: 1, pageSize: 10, totalCount: 3);

        // Assert
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public void Constructor_EmptyItems_ShouldHandleGracefully()
    {
        // Arrange
        var items = new List<string>();

        // Act
        var result = new PagedResult<string>(items, page: 1, pageSize: 10, totalCount: 0);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalPages.Should().Be(0);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void TotalPages_WithExactDivision_ShouldCalculateCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = new PagedResult<int>(items, page: 1, pageSize: 5, totalCount: 15);

        // Assert
        result.TotalPages.Should().Be(3); // 15 / 5 = 3
    }

    [Fact]
    public void TotalPages_WithRemainder_ShouldRoundUp()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };

        // Act
        var result = new PagedResult<int>(items, page: 1, pageSize: 7, totalCount: 20);

        // Assert
        result.TotalPages.Should().Be(3); // Math.Ceiling(20 / 7) = 3
    }

    [Fact]
    public void Create_WithDifferentType_ShouldWork()
    {
        // Arrange
        var items = new List<TestDto> 
        { 
            new("Test1"),
            new("Test2")
        };

        // Act
        var result = PagedResult<TestDto>.Create(items, page: 1, pageSize: 10, totalCount: 2);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.First().Name.Should().Be("Test1");
    }

    [Fact]
    public void Items_ShouldBeReadOnly()
    {
        // Arrange
        var items = new List<string> { "a", "b", "c" };
        var result = PagedResult<string>.Create(items, 1, 10, 3);

        // Assert
        result.Items.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    [Theory]
    [InlineData(1, 10, 100, 10, false, true)]  // First page
    [InlineData(5, 10, 100, 10, true, true)]   // Middle page
    [InlineData(10, 10, 100, 10, true, false)] // Last page
    [InlineData(1, 25, 50, 2, false, true)]    // Custom page size
    public void Constructor_VariousScenarios_ShouldCalculateCorrectly(
        int page, int pageSize, int totalCount, int expectedTotalPages, bool expectedHasPrevious, bool expectedHasNext)
    {
        // Arrange
        var items = new List<string> { "item" };

        // Act
        var result = new PagedResult<string>(items, page, pageSize, totalCount);

        // Assert
        result.TotalPages.Should().Be(expectedTotalPages);
        result.HasPreviousPage.Should().Be(expectedHasPrevious);
        result.HasNextPage.Should().Be(expectedHasNext);
    }

    private record TestDto(string Name);
}
