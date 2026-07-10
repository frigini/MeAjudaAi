using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Database.Constants;

[ExcludeFromCodeCoverage]
public static class DatabaseConstants
{
    public const string DefaultTestConnectionString = "Host=localhost;Database=test;Username=test;";
    public const string LocalConnectionString = "Host=localhost;Database=test";
    public const string LocalWithCredentialsConnectionString = "Host=localhost;Database=test;Username=postgres;Password=test";
    public const string LocalWithPortConnectionString = "Host=localhost;Port=5432;Database=test;Username=test;Password=test;";
    public const string AspireLocalConnectionString = "Host=aspire-local;Database=local";
    public const string AspireDevConnectionString = "Host=aspire-dev;Database=dev";
    public const string DummyConnectionString = "Host=localhost;Database=dummy;Username=postgres;Password=postgres";
    public const string InvalidConnectionString = "Host=invalid;Port=9999;Database=invalid;Username=invalid;Password=invalid;";
}
