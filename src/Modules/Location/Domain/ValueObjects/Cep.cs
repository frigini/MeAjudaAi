using System.Text.RegularExpressions;

namespace MeAjudaAi.Modules.Location.Domain.ValueObjects;

/// <summary>
/// Representa um Código de Endereçamento Postal (CEP) brasileiro.
/// </summary>
public sealed partial class Cep : IEquatable<Cep>
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

        var cleanedValue = value.Replace("-", "").Replace(".", "").Trim();

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

    public bool Equals(Cep? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => obj is Cep other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Cep? left, Cep? right) => Equals(left, right);

    public static bool operator !=(Cep? left, Cep? right) => !Equals(left, right);

    [GeneratedRegex(@"^\d{8}$", RegexOptions.Compiled)]
    private static partial Regex CepPattern();
}
