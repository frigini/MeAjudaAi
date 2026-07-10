using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks;

public class MockLocalizerBuilder
{
    private readonly Mock<IStringLocalizer<Strings>> _mock = new();
    private readonly Dictionary<string, string> _simpleKeys = new();
    private readonly Dictionary<string, Func<string, object[], string>> _formattedKeys = new();
    private bool _returnKeyAsValue;

    public static MockLocalizerBuilder Create() => new();

    public MockLocalizerBuilder WithSimpleKey(string key, string value)
    {
        _simpleKeys[key] = value;
        return this;
    }

    public MockLocalizerBuilder WithFormattedKey(string key, Func<string, object[], string> formatter)
    {
        _formattedKeys[key] = formatter;
        return this;
    }

    public MockLocalizerBuilder ReturnKeyAsValue()
    {
        _returnKeyAsValue = true;
        return this;
    }

    public MockLocalizerBuilder WithProviderNotFound() =>
        WithSimpleKey("ProviderNotFound", "Prestador não encontrado.");

    public MockLocalizerBuilder WithBookingNotFound() =>
        WithSimpleKey("BookingNotFound", "Agendamento não encontrado.");

    public MockLocalizerBuilder WithDocumentNotFound() =>
        WithFormattedKey("DocumentNotFound", (key, args) => $"Documento com ID {args[0]} não encontrado.");

    public MockLocalizerBuilder WithTemplateNotFound() =>
        WithSimpleKey("TemplateNotFound", "Template de e-mail não encontrado.");

    public MockLocalizerBuilder WithReviewNotFound() =>
        WithSimpleKey("ReviewNotFound", "Avaliação não encontrada.");

    public MockLocalizerBuilder WithHttpContextNotAvailable() =>
        WithSimpleKey("HttpContextNotAvailable", "Contexto HTTP não disponível.");

    public MockLocalizerBuilder WithUserNotAuthenticated() =>
        WithSimpleKey("UserNotAuthenticated", "Usuário não autenticado.");

    public MockLocalizerBuilder WithServiceNotFoundById() =>
        WithFormattedKey("ServiceNotFoundById", (key, args) => $"Serviço com ID '{args[0]}' não encontrado.");

    public MockLocalizerBuilder WithCategoryNotFoundById() =>
        WithFormattedKey("CategoryNotFoundById", (key, args) => $"Categoria com ID '{args[0]}' não encontrada.");

    public Mock<IStringLocalizer<Strings>> Build()
    {
        foreach (var (key, value) in _simpleKeys)
        {
            var k = key;
            var v = value;
            _mock.Setup(x => x[It.Is<string>(s => s == k)])
                .Returns(new LocalizedString(k, v));
        }

        foreach (var (key, formatter) in _formattedKeys)
        {
            var k = key;
            var f = formatter;
            _mock.Setup(x => x[It.Is<string>(s => s == k), It.IsAny<object[]>()])
                .Returns((string keyName, object[] args) => new LocalizedString(keyName, f(keyName, args)));
        }

        if (_returnKeyAsValue)
        {
            _mock.Setup(l => l[It.IsAny<string>()])
                .Returns((string name) => new LocalizedString(name, name));
        }

        return _mock;
    }
}
