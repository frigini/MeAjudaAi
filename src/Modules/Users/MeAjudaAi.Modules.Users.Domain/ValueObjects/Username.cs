using System.Text.RegularExpressions;

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
        if (value.Length < 3)
            throw new ArgumentException("Username deve ter pelo menos 3 caracteres", nameof(value));
        if (value.Length > 30)
            throw new ArgumentException("Username não pode ter mais de 30 caracteres", nameof(value));
        if (!UsernameRegex.IsMatch(value))
            throw new ArgumentException("Username contém caracteres inválidos", nameof(value));
        Value = value.ToLowerInvariant();
    }

    public static implicit operator string(Username username) => username.Value;
    public static implicit operator Username(string username) => new(username);

    [GeneratedRegex(@"^[a-zA-Z0-9._-]{3,30}$", RegexOptions.Compiled)]
    private static partial Regex UsernameGeneratedRegex();
}