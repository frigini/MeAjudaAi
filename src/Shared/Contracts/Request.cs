namespace MeAjudaAi.Shared.Contracts;

public abstract record Request
{
    public string? UserId { get; init; }
}
