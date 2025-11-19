namespace MeAjudaAi.Shared.Modules;

/// <summary>
/// Informações sobre uma Module API registrada.
/// </summary>
/// <param name="ModuleName">Nome do módulo</param>
/// <param name="ApiVersion">Versão da API</param>
/// <param name="ImplementationType">Tipo completo da implementação (formato: Namespace.TypeName, AssemblyName)</param>
/// <param name="IsAvailable">Indica se o módulo está disponível e saudável</param>
public sealed record ModuleApiInfo(
    string ModuleName,
    string ApiVersion,
    string ImplementationType,
    bool IsAvailable
);
