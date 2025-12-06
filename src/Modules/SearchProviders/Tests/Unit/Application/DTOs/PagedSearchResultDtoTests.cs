using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using Xunit;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Application.DTOs;

public class PagedSearchResultDtoTests
{
    [Fact]
    public void TotalPages_WithValidPageSize_ShouldCalculateCorrectly()
    {
        // Arrange & Act
        var result = new PagedSearchResultDto<string>
        {
            Items = new List<string> { "item1", "item2" },
            TotalCount = 100,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        result.TotalPages.Should().Be(10);
    }

    [Fact]
    public void TotalPages_WithPageSizeZero_ShouldReturnZero()
    {
        // Arrange & Act
        var result = new PagedSearchResultDto<string>
        {
            Items = new List<string>(),
            TotalCount = 100,
            PageNumber = 1,
            PageSize = 0
        };

        // Assert
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void TotalPages_WithNegativePageSize_ShouldReturnZero()
    {
        // Arrange & Act
        var result = new PagedSearchResultDto<string>
        {
            Items = new List<string>(),
            TotalCount = 100,
            PageNumber = 1,
            PageSize = -5
        };

        // Assert
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void TotalPages_WhenTotalCountNotDivisibleByPageSize_ShouldRoundUp()
    {
        // Arrange & Act
        var result = new PagedSearchResultDto<string>
        {
            Items = new List<string>(),
            TotalCount = 95,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        result.TotalPages.Should().Be(10); // 95 / 10 = 9.5 -> rounds up to 10
    }

    [Fact]
    public void HasNextPage_WhenOnFirstPageWithMultiplePages_ShouldReturnTrue()
    {
        // Arrange & Act
        var result = new PagedSearchResultDto<string>
        {
            Items = new List<string>(),
            TotalCount = 100,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_WhenOnLastPage_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = new PagedSearchResultDto<string>
        {
            Items = new List<string>(),
            TotalCount = 100,
            PageNumber = 10,
            PageSize = 10
        };

        // Assert
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_WhenOnlyOnePage_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = new PagedSearchResultDto<string>
        {
            Items = new List<string>(),
            TotalCount = 5,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_WhenTotalPagesIsZero_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = new PagedSearchResultDto<string>
        {
            Items = new List<string>(),
            TotalCount = 100,
            PageNumber = 1,
            PageSize = 0
        };

        // Assert
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_WhenOnFirstPage_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = new PagedSearchResultDto<string>
        {
            Items = new List<string>(),
            TotalCount = 100,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_WhenOnSecondPage_ShouldReturnTrue()
    {
        // Arrange & Act
        var result = new PagedSearchResultDto<string>
        {
            Items = new List<string>(),
            TotalCount = 100,
            PageNumber = 2,
            PageSize = 10
        };

        // Assert
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasPreviousPage_WhenOnLastPage_ShouldReturnTrue()
    {
        // Arrange & Act
        var result = new PagedSearchResultDto<string>
        {
            Items = new List<string>(),
            TotalCount = 100,
            PageNumber = 10,
            PageSize = 10
        };

        // Assert
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasPreviousPage_WhenTotalPagesIsZero_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = new PagedSearchResultDto<string>
        {
            Items = new List<string>(),
            TotalCount = 100,
            PageNumber = 5,
            PageSize = 0
        };

        // Assert
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void Items_ShouldStoreProvidedCollection()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };

        // Act
        var result = new PagedSearchResultDto<string>
        {
            Items = items,
            TotalCount = 3,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void Properties_ShouldMatchProvidedValues()
    {
        // Arrange & Act
        var result = new PagedSearchResultDto<int>
        {
            Items = new List<int> { 1, 2, 3 },
            TotalCount = 250,
            PageNumber = 5,
            PageSize = 25
        };

        // Assert
        result.TotalCount.Should().Be(250);
        result.PageNumber.Should().Be(5);
        result.PageSize.Should().Be(25);
    }
}
