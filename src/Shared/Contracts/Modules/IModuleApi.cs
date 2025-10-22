namespace MeAjudaAi.Shared.Contracts.Modules;

/// <summary>
/// Interface base para todas as APIs de módulos
/// </summary>
public interface IModuleApi
{
    /// <summary>
    /// Nome do módulo
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Versão da API do módulo
    /// </summary>
    string ApiVersion { get; }

    /// <summary>
    /// Verifica se o módulo está disponível
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Attribute para marcar uma implementação de Module API
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ModuleApiAttribute(string moduleName, string apiVersion = "1.0") : Attribute
{
    public string ModuleName { get; } = moduleName;
    public string ApiVersion { get; } = apiVersion;
}
