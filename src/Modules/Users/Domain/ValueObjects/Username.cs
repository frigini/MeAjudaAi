using System.Text.RegularExpressions;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Nome de usuário.
/// </summary>
public sealed partial record Username
{
    private static readonly Regex UsernameRegex = UsernameGeneratedRegex();
    public string Value { get; }

    public Username(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Username não pode ser vazio", nameof(value));
        if (value.Length < ValidationConstants.UserLimits.UsernameMinLength)
            throw new ArgumentException($"Username deve ter pelo menos {ValidationConstants.UserLimits.UsernameMinLength} caracteres", nameof(value));
        if (value.Length > ValidationConstants.UserLimits.UsernameMaxLength)
            throw new ArgumentException($"Username não pode ter mais de {ValidationConstants.UserLimits.UsernameMaxLength} caracteres", nameof(value));
        if (!UsernameRegex.IsMatch(value))
            throw new ArgumentException("Username contém caracteres inválidos", nameof(value));
        Value = value.ToLowerInvariant();
    }

    public static implicit operator string(Username username) => username.Value;
    public static implicit operator Username(string username) => new(username);

    [GeneratedRegex(@"^[a-zA-Z0-9._-]{3,30}$", RegexOptions.Compiled)]
    private static partial Regex UsernameGeneratedRegex();
}
