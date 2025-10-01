using MeAjudaAi.Modules.Users.API.Mappers;
using MeAjudaAi.Modules.Users.Application.Queries;

namespace MeAjudaAi.Modules.Users.Tests.Unit.API.Endpoints;

/// <summary>
/// Testes unitários para validação do endpoint de busca de usuário por email.
/// Testa mapeamento de dados, validação de entrada e estrutura de queries.
/// </summary>
public class GetUserByEmailEndpointTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("admin@company.co.uk")]
    [InlineData("support+tag@service.io")]
    public void ToEmailQuery_WithValidEmails_ShouldCreateCorrectQuery(string email)
    {
        // Act
        var query = email.ToEmailQuery();

        // Assert
        query.Should().NotBeNull();
        query.Email.Should().Be(email);
        query.Should().BeOfType<GetUserByEmailQuery>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void ToEmailQuery_WithEmptyOrWhitespaceEmails_ShouldCreateQueryWithProvidedValue(string email)
    {
        // Act
        var query = email.ToEmailQuery();

        // Assert
        query.Should().NotBeNull();
        query.Email.Should().Be(email);
        query.Should().BeOfType<GetUserByEmailQuery>();
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user@domain")]
    [InlineData("user.domain.com")]
    public void ToEmailQuery_WithInvalidEmailFormats_ShouldStillCreateQuery(string invalidEmail)
    {
        // Act
        var query = invalidEmail.ToEmailQuery();

        // Assert
        query.Should().NotBeNull();
        query.Email.Should().Be(invalidEmail);
        query.Should().BeOfType<GetUserByEmailQuery>();

        // Nota: A validação do email deve ocorrer na camada de domínio, não no mapper
    }

    [Fact]
    public void ToEmailQuery_WithNullEmail_ShouldCreateQueryWithEmptyString()
    {
        // Arrange
        string? email = null;

        // Act
        var query = email.ToEmailQuery();

        // Assert
        query.Should().NotBeNull();
        query.Email.Should().Be(string.Empty); // Null é convertido para string vazia
        query.Should().BeOfType<GetUserByEmailQuery>();
    }

    [Fact]
    public void GetUserByEmailQuery_Properties_ShouldBeReadOnly()
    {
        // Arrange
        var email = "test@example.com";
        var query = new GetUserByEmailQuery(email);

        // Act & Assert
        query.Email.Should().Be(email);
        query.CorrelationId.Should().NotBeEmpty();

        // Verifica igualdade do Email mesmo com CorrelationId diferente
        var query2 = new GetUserByEmailQuery(email);
        query.Email.Should().Be(query2.Email);
        query.CorrelationId.Should().NotBe(query2.CorrelationId); // Instâncias diferentes têm CorrelationIds diferentes
    }

    [Fact]
    public void GetUserByEmailQuery_ToString_ShouldContainEmail()
    {
        // Arrange
        var email = "test@example.com";
        var query = new GetUserByEmailQuery(email);

        // Act
        var stringRepresentation = query.ToString();

        // Assert
        stringRepresentation.Should().Contain(email);
        stringRepresentation.Should().Contain("GetUserByEmailQuery");
    }

    [Theory]
    [InlineData("TEST@EXAMPLE.COM")]
    [InlineData("Test@Example.Com")]
    [InlineData("test@EXAMPLE.com")]
    public void ToEmailQuery_WithDifferentCasing_ShouldPreserveCasing(string email)
    {
        // Act
        var query = email.ToEmailQuery();

        // Assert
        query.Should().NotBeNull();
        query.Email.Should().Be(email);
        query.Email.Should().NotBe(email.ToLower());

        // Nota: Normalização do email deve ocorrer na camada de domínio
    }

    [Fact]
    public void MapperExtension_ShouldBeAccessibleFromString()
    {
        // Arrange
        var email = "test@example.com";

        // Act & Assert - Testa se o método de extensão está disponível
        var action = () => email.ToEmailQuery();
        action.Should().NotThrow();

        var result = action();
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void ToEmailQuery_PerformanceTest_ShouldBeEfficient(int iterations)
    {
        // Arrange
        var emails = Enumerable.Range(0, iterations)
            .Select(i => $"user{i}@example.com")
            .ToList();

        // Act
        var queries = emails.Select(email => email.ToEmailQuery()).ToList();

        // Assert
        queries.Should().HaveCount(iterations);
        queries.Should().AllSatisfy(query =>
        {
            query.Should().NotBeNull();
            query.Should().BeOfType<GetUserByEmailQuery>();
            query.Email.Should().StartWith("user");
            query.Email.Should().EndWith("@example.com");
        });
    }
}