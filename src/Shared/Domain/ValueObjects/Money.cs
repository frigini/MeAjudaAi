namespace MeAjudaAi.Shared.Domain.ValueObjects;

public record Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency = "BRL") => new(0, currency);

    public static Money FromDecimal(decimal amount, string currency = "BRL") => new(amount, currency);

    public override string ToString() => $"{Currency} {Amount:N2}";

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies.");
        return new Money(a.Amount + b.Amount, a.Currency);
    }
}
