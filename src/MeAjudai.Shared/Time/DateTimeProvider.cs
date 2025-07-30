namespace MeAjudaAi.Shared.Time
{
    internal sealed class DateTimeProvider : IDateTimeProvider
    {
        public DateTime CurrentDate() => DateTime.UtcNow;
    }
}