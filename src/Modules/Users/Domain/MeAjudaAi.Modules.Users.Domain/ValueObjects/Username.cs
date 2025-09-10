using System.Text.RegularExpressions;

namespace MeAjudaAi.Modules.Users.Domain.ValueObjects;

public sealed partial record Username
{
    private static readonly Regex UsernameRegex = UsernameGeneratedRegex();

    public string Value { get; }

    public Username(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Username cannot be empty", nameof(value));

        if (value.Length < 3)
            throw new ArgumentException("Username must be at least 3 characters", nameof(value));

        if (value.Length > 30)
            throw new ArgumentException("Username cannot exceed 30 characters", nameof(value));

        if (!UsernameRegex.IsMatch(value))
            throw new ArgumentException("Username contains invalid characters", nameof(value));

        Value = value.ToLowerInvariant();
    }

    public static implicit operator string(Username username) => username.Value;
    public static implicit operator Username(string username) => new(username);

    [GeneratedRegex(@"^[a-zA-Z0-9._-]{3,30}$", RegexOptions.Compiled)]
    private static partial Regex UsernameGeneratedRegex();
}S