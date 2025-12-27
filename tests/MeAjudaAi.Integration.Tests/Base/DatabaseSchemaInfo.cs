namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Informações sobre um schema em cache
/// </summary>
public class DatabaseSchemaInfo
{
    public string SchemaHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastSuccessfulInit { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public bool IsInitialized { get; set; }
}
