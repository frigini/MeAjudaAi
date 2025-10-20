using System.Text.RegularExpressions;
using MeAjudaAi.Shared.Constants;

namespace MeAjudaAi.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Endereço de email.
/// </summary>
public sealed partial record Email
{
    private static readonly Regex EmailRegex = EmailGeneratedRegex();
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email não pode ser vazio", nameof(value));
        if (value.Length > ValidationConstants.UserLimits.EmailMaxLength)
            throw new ArgumentException($"Email não pode ter mais de {ValidationConstants.UserLimits.EmailMaxLength} caracteres", nameof(value));
        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException("Formato de email inválido", nameof(value));
        Value = value.ToLowerInvariant();
    }

    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string email) => new(email);

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex EmailGeneratedRegex();
}
