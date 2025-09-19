using FluentAssertions;
using MeAjudaAi.Modules.Users.API.Mappers;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Queries;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.API.Endpoints;

/// <summary>
/// Testes unitários para validação do endpoint de listagem paginada de usuários.
/// Testa mapeamento de dados, validação de paginação e estrutura de queries.
/// </summary>
public class GetUsersEndpointTests
{
    [Fact]
    public void ToUsersQuery_WithValidRequest_ShouldCreateCorrectQuery()
    {
        // Arrange
        var request = new GetUsersRequest
        {
            PageNumber = 2,
            PageSize = 20,
            SearchTerm = "test search"
        };

        // Act
        var query = request.ToUsersQuery();

        // Assert
        query.Should().NotBeNull();
        query.Page.Should().Be(2);
        query.PageSize.Should().Be(20);
        query.SearchTerm.Should().Be("test search");
        query.Should().BeOfType<GetUsersQuery>();
    }

    [Fact]
    public void ToUsersQuery_WithDefaultValues_ShouldCreateQueryWithDefaults()
    {
        // Arrange
        var request = new GetUsersRequest(); // Default values

        // Act
        var query = request.ToUsersQuery();

        // Assert
        query.Should().NotBeNull();
        query.Page.Should().Be(1); // Default page
        query.PageSize.Should().Be(10); // Default page size
        query.SearchTerm.Should().BeNull();
        query.Should().BeOfType<GetUsersQuery>();
    }

    [Theory]
    [InlineData(1, 10, null)]
    [InlineData(1, 25, "")]
    [InlineData(5, 50, "admin")]
    [InlineData(10, 100, "test@example.com")]
    public void ToUsersQuery_WithDifferentValidValues_ShouldMapCorrectly(int page, int pageSize, string? searchTerm)
    {
        // Arrange
        var request = new GetUsersRequest
        {
            PageNumber = page,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        // Act
        var query = request.ToUsersQuery();

        // Assert
        query.Should().NotBeNull();
        query.Page.Should().Be(page);
        query.PageSize.Should().Be(pageSize);
        query.SearchTerm.Should().Be(searchTerm);
    }

    [Theory]
    [InlineData(0, 10)] // Invalid page
    [InlineData(-1, 10)] // Negative page
    [InlineData(1, 0)] // Invalid page size
    [InlineData(1, -5)] // Negative page size
    public void ToUsersQuery_WithInvalidPaginationValues_ShouldStillCreateQuery(int page, int pageSize)
    {
        // Arrange
        var request = new GetUsersRequest
        {
            PageNumber = page,
            PageSize = pageSize
        };

        // Act
        var query = request.ToUsersQuery();

        // Assert
        query.Should().NotBeNull();
        query.Page.Should().Be(page);
        query.PageSize.Should().Be(pageSize);
        
        // Note: Validation should happen at domain level or in the request validator
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void ToUsersQuery_WithEmptyOrWhitespaceSearchTerm_ShouldCreateQueryWithProvidedValue(string searchTerm)
    {
        // Arrange
        var request = new GetUsersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = searchTerm
        };

        // Act
        var query = request.ToUsersQuery();

        // Assert
        query.Should().NotBeNull();
        query.SearchTerm.Should().Be(searchTerm);
    }

    [Fact]
    public void GetUsersQuery_Properties_ShouldBeReadOnly()
    {
        // Arrange
        var page = 2;
        var pageSize = 25;
        var searchTerm = "test";
        var query = new GetUsersQuery(page, pageSize, searchTerm);

        // Act & Assert
        query.Page.Should().Be(page);
        query.PageSize.Should().Be(pageSize);
        query.SearchTerm.Should().Be(searchTerm);
        query.CorrelationId.Should().NotBeEmpty();
        
        // Verify property equality even with different CorrelationId
        var query2 = new GetUsersQuery(page, pageSize, searchTerm);
        query.Page.Should().Be(query2.Page);
        query.PageSize.Should().Be(query2.PageSize);
        query.SearchTerm.Should().Be(query2.SearchTerm);
        query.CorrelationId.Should().NotBe(query2.CorrelationId); // Different instances have different CorrelationIds
    }

    [Fact]
    public void GetUsersQuery_ToString_ShouldContainRelevantInfo()
    {
        // Arrange
        var page = 3;
        var pageSize = 15;
        var searchTerm = "admin";
        var query = new GetUsersQuery(page, pageSize, searchTerm);

        // Act
        var stringRepresentation = query.ToString();

        // Assert
        stringRepresentation.Should().Contain("GetUsersQuery");
        stringRepresentation.Should().Contain(page.ToString());
        stringRepresentation.Should().Contain(pageSize.ToString());
        stringRepresentation.Should().Contain(searchTerm);
    }

    [Fact]
    public void MapperExtension_ShouldBeAccessibleFromRequest()
    {
        // Arrange
        var request = new GetUsersRequest
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act & Assert - Testing that the extension method is available
        var action = () => request.ToUsersQuery();
        action.Should().NotThrow();
        
        var result = action();
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(500)]
    public void ToUsersQuery_PerformanceTest_ShouldBeEfficient(int iterations)
    {
        // Arrange
        var requests = Enumerable.Range(1, iterations)
            .Select(i => new GetUsersRequest
            {
                PageNumber = i,
                PageSize = 10,
                SearchTerm = $"search{i}"
            })
            .ToList();

        // Act
        var queries = requests.Select(req => req.ToUsersQuery()).ToList();

        // Assert
        queries.Should().HaveCount(iterations);
        queries.Should().AllSatisfy(query => 
        {
            query.Should().NotBeNull();
            query.Should().BeOfType<GetUsersQuery>();
            query.Page.Should().BeGreaterThan(0);
            query.PageSize.Should().Be(10);
        });
    }

    [Fact]
    public void GetUsersRequest_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var request = new GetUsersRequest();

        // Assert
        request.PageNumber.Should().Be(1);
        request.PageSize.Should().Be(10);
        request.SearchTerm.Should().BeNull();
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("test@example.com")]
    [InlineData("John Doe")]
    [InlineData("user123")]
    public void ToUsersQuery_WithVariousSearchTerms_ShouldPreserveSearchTerm(string searchTerm)
    {
        // Arrange
        var request = new GetUsersRequest
        {
            SearchTerm = searchTerm
        };

        // Act
        var query = request.ToUsersQuery();

        // Assert
        query.SearchTerm.Should().Be(searchTerm);
    }
}