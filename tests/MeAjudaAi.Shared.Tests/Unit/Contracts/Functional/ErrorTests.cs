using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Shared.Tests.Unit.Contracts.Functional;

[Trait("Category", "Unit")]
public class ErrorTests
{
    [Fact]
    public void Unprocessable_ShouldReturnErrorWithStatusCode422()
    {
        var message = "Categoria inativa";

        var error = Error.Unprocessable(message);

        error.StatusCode.Should().Be(422);
        error.Message.Should().Be(message);
        error.Code.Should().BeNull();
    }

    [Fact]
    public void Unprocessable_WithCode_ShouldIncludeCode()
    {
        var message = "Entidade não processável";
        var code = "UNPROCESSABLE";

        var error = Error.Unprocessable(message, code);

        error.StatusCode.Should().Be(422);
        error.Message.Should().Be(message);
        error.Code.Should().Be(code);
    }

    [Fact]
    public void NotFound_ShouldReturnErrorWithStatusCode404()
    {
        var message = "Recurso não encontrado";
        var error = Error.NotFound(message);

        error.StatusCode.Should().Be(404);
        error.Message.Should().Be(message);
        error.Code.Should().BeNull();
    }

    [Fact]
    public void BadRequest_ShouldReturnErrorWithStatusCode400()
    {
        var message = "Requisição inválida";
        var error = Error.BadRequest(message);

        error.StatusCode.Should().Be(400);
        error.Message.Should().Be(message);
        error.Code.Should().BeNull();
    }

    [Fact]
    public void Conflict_ShouldReturnErrorWithStatusCode409()
    {
        var message = "Conflito detectado";
        var error = Error.Conflict(message);

        error.StatusCode.Should().Be(409);
        error.Message.Should().Be(message);
        error.Code.Should().BeNull();
    }
}
