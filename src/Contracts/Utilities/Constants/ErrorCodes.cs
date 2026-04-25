namespace MeAjudaAi.Contracts.Utilities.Constants;

/// <summary>
/// Códigos de erro estáveis para identificação programática.
/// </summary>
public static class ErrorCodes
{
    public const string InternalError = "internal_error";
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";
    public const string NotFound = "not_found";
    public const string BadRequest = "bad_request";
    public const string Conflict = "conflict";
    public const string Validation = "validation_error";

    public static class Providers
    {
        public const string ProviderNotFound = "provider_not_found";
        public const string ServiceNotOffered = "service_not_offered";
        public const string ScheduleNotFound = "schedule_not_found";
        public const string Unavailable = "provider_unavailable";
    }

    public static class Bookings
    {
        public const string Overlap = "booking_overlap";
        public const string ConcurrencyConflict = "booking_concurrency_conflict";
        public const string InvalidTime = "invalid_booking_time";
        public const string MidnightSpanning = "midnight_spanning";
        public const string StartNotInFuture = "start_not_in_future";
    }

    public static class Catalogs
    {
        public const string ServiceNotFound = "service_not_found";
        public const string ServiceInactive = "service_inactive";
    }
}
