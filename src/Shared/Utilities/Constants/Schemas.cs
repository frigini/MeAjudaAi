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
}
