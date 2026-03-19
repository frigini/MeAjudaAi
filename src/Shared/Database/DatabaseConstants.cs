namespace MeAjudaAi.Shared.Database;

public static class DatabaseConstants
{
    /// <summary>
    /// String de conexão padrão para ambientes de teste e desenvolvimento local (fallback).
    /// </summary>
#pragma warning disable S2068 // "password" detected here, make sure this is not a hard-coded credential
    public const string DefaultTestConnectionString = "Host=localhost;Database=test;Username=test;Password=test";
#pragma warning restore S2068 // "password" detected here, make sure this is not a hard-coded credential
}
