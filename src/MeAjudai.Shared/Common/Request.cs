namespace MeAjudai.Shared.Common;

public abstract record Request
{
    public string? UserId { get; init; }
}