using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes para nomes de schemas do banco de dados.
/// Usado em HasDefaultSchema e ToTable para garantir consistência entre módulos.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Schemas
{
    public const string Users = "users";
    public const string Providers = "providers";
    public const string Documents = "documents";
    public const string ServiceCatalogs = "service_catalogs";
    public const string SearchProviders = "search_providers";
    public const string Locations = "locations";
    public const string Bookings = "bookings";
    public const string Communications = "communications";
    public const string Payments = "payments";
    public const string Ratings = "ratings";

    /// <summary>
    /// Nomes dos roles de banco de dados por schema (para schema isolation).
    /// </summary>
    public static class Roles
    {
        public const string Users = "users_role";
        public const string Providers = "providers_role";
        public const string Documents = "documents_role";
        public const string ServiceCatalogs = "service_catalogs_role";
        public const string SearchProviders = "search_providers_role";
        public const string Locations = "locations_role";
        public const string Bookings = "bookings_role";
        public const string Communications = "communications_role";
        public const string Payments = "payments_role";
        public const string Ratings = "ratings_role";
    }
}
