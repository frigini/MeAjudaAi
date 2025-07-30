namespace MeAjudai.Shared.Time
{
    internal sealed class DateTimeProvider : IDateTimeProvider
    {
        public DateTime CurrentDate() => DateTime.UtcNow;
    }
}