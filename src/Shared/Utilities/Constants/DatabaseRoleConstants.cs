using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes para nomes de roles do PostgreSQL usados no schema isolation.
/// Cada módulo tem um role dedicado para acesso ao seu schema.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DatabaseRoleConstants
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
