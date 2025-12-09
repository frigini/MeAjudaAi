using System.Text.RegularExpressions;
using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Locations.Domain.ValueObjects;

/// <summary>
/// Representa um Código de Endereçamento Postal (CEP) brasileiro.
/// </summary>
public sealed partial class Cep : ValueObject
{
    private static readonly Regex CepRegex = CepPattern();

    public string Value { get; }

    private Cep(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Cria um CEP a partir de uma string, validando o formato.
    /// Aceita formatos: 12345678, 12345-678
    /// </summary>
    public static Cep? Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var cleanedValue = value.Replace("-", "").Replace(".", "").Replace(" ", "").Trim();

        if (!CepRegex.IsMatch(cleanedValue))
        {
            return null;
        }

        return new Cep(cleanedValue);
    }

    /// <summary>
    /// Retorna o CEP formatado: 12345-678
    /// </summary>
    public string Formatted => Value.Length == 8
        ? $"{Value[..5]}-{Value[5..]}"
        : Value;

    public override string ToString() => Formatted;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^\d{8}$", RegexOptions.Compiled)]
    private static partial Regex CepPattern();
}
