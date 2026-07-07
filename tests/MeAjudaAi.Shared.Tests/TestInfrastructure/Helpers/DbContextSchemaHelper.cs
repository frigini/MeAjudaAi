using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Helpers;

/// <summary>
/// Mapeia nomes de DbContext para nomes de schema do PostgreSQL.
/// Centraliza o mapeamento contexto→schema para uso em testes de integração e E2E.
/// </summary>
public static class DbContextSchemaHelper
{
    private static readonly Dictionary<string, string> ContextToSchemaMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["UsersDbContext"] = Schemas.Users,
        ["ProvidersDbContext"] = Schemas.Providers,
        ["DocumentsDbContext"] = Schemas.Documents,
        ["ServiceCatalogsDbContext"] = Schemas.ServiceCatalogs,
        ["LocationsDbContext"] = Schemas.Locations,
        ["CommunicationsDbContext"] = Schemas.Communications,
        ["SearchProvidersDbContext"] = Schemas.SearchProviders,
        ["RatingsDbContext"] = Schemas.Ratings,
        ["PaymentsDbContext"] = Schemas.Payments,
        ["BookingsDbContext"] = Schemas.Bookings,
    };

    /// <summary>
    /// Obtém o nome do schema para um DbContext pelo nome do tipo.
    /// </summary>
    public static string GetSchemaName(string contextName)
    {
        return ContextToSchemaMap.TryGetValue(contextName, out var schema) ? schema : "public";
    }

    /// <summary>
    /// Obtém o nome do schema para um DbContext pelo tipo.
    /// </summary>
    public static string GetSchemaName(Type contextType)
    {
        return GetSchemaName(contextType.Name);
    }

    /// <summary>
    /// Retorna todos os schemas de módulos conhecidos (exclui "public").
    /// </summary>
    public static IReadOnlyCollection<string> GetAllModuleSchemas()
    {
        return ContextToSchemaMap.Values.Distinct().ToList().AsReadOnly();
    }
}
