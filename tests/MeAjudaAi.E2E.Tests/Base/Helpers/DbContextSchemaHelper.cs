namespace MeAjudaAi.E2E.Tests.Base.Helpers;

public static class DbContextSchemaHelper
{
    public static string GetSchemaName(string contextName)
    {
        return contextName switch
        {
            "UsersDbContext" => "users",
            "ProvidersDbContext" => "providers",
            "DocumentsDbContext" => "documents",
            "ServiceCatalogsDbContext" => "service_catalogs",
            "LocationsDbContext" => "locations",
            "CommunicationsDbContext" => "communications",
            "SearchProvidersDbContext" => "search_providers",
            "RatingsDbContext" => "ratings",
            _ => "public"
        };
    }
}
