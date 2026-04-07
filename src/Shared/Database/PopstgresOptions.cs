using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Database;

[ExcludeFromCodeCoverage]
public sealed class PostgresOptions
{
    public const string SectionName = "Postgres";
    public string ConnectionString { get; set; } = string.Empty;
}
