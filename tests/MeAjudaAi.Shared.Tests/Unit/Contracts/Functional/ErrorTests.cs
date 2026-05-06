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
        var error = Error.NotFound("Recurso não encontrado");

        error.StatusCode.Should().Be(404);
    }

    [Fact]
    public void BadRequest_ShouldReturnErrorWithStatusCode400()
    {
        var error = Error.BadRequest("Requisição inválida");

        error.StatusCode.Should().Be(400);
    }

    [Fact]
    public void Conflict_ShouldReturnErrorWithStatusCode409()
    {
        var error = Error.Conflict("Conflito detectado");

        error.StatusCode.Should().Be(409);
    }
}
