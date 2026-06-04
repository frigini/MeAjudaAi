using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Caching;

/// <summary>
/// Constantes para tags de cache utilizadas no sistema.
/// Permite invalidação em grupo de entradas relacionadas.
/// </summary>
[ExcludeFromCodeCoverage]
public static class CacheTags
{
    // Tags para o módulo Users
    public const string Users = "users";
    public static string UserTag(Guid userId) => $"user:{userId}";
    public static string UserEmailTag(string email) => $"user-email:{email.ToLowerInvariant()}";
    public static string UsersPageTag(int page, int pageSize) => $"users-page:{page}:{pageSize}";
    public static string[] GetUserRelatedTags(Guid userId, string? email = null)
    {
        var tags = new List<string> { Users, UserTag(userId) };
        if (!string.IsNullOrEmpty(email)) tags.Add(UserEmailTag(email));
        return [.. tags];
    }

    // Tags para o módulo Bookings
    public const string Bookings = "bookings";
    public static string BookingTag(Guid id) => $"booking:{id}";
    public static string ProviderBookingsTag(Guid providerId) => $"provider-bookings:{providerId}";

    // Tags para o módulo Payments
    public const string Payments = "payments";
    public static string PaymentTag(Guid id) => $"payment:{id}";
    public static string SubscriptionTag(Guid id) => $"subscription:{id}";

    // Tags para o módulo Communications
    public const string Communications = "communications";
    public const string EmailTemplates = "email-templates";
    public static string EmailTemplateTag(string key) => $"email-template:{key}";

    // Tags para o módulo Providers
    public const string Providers = "providers";
    public static string ProviderTag(Guid id) => $"provider:{id}";
    public const string ProvidersList = "providers-list";

    // Tags para o módulo Documents
    public const string Documents = "documents";
    public static string DocumentTag(Guid id) => $"document:{id}";

    // Tags para o módulo Locations
    public const string Locations = "locations";
    public static string CepTag(string cep) => $"cep:{cep}";
    public static string MunicipioTag(string municipio) => $"municipio:{municipio}";

    // Tags para o módulo Search
    public const string Search = "search";
    public const string SearchResults = "search-results";

    // Tags para o módulo ServiceCatalogs
    public const string ServiceCatalogs = "service-catalogs";
    public static string CategoryTag(Guid id) => $"category:{id}";
    public static string ServiceTag(Guid id) => $"service:{id}";

    // Tags gerais do sistema
    public const string Configuration = "configuration";
    public const string Metadata = "metadata";

    /// <summary>
    /// Combina múltiplas tags
    /// </summary>
    public static string[] CombineTags(params string[] tags) => tags;
}
