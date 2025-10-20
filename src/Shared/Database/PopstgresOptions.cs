namespace MeAjudaAi.Shared.Database;

public sealed class PostgresOptions
{
    public const string SectionName = "Postgres";
    public string ConnectionString { get; set; } = string.Empty;
}
