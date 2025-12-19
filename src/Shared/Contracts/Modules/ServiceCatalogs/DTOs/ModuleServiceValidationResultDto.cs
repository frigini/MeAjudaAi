namespace MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs.DTOs;

/// <summary>
/// Resultado da operação de validação de serviços.
/// </summary>
public sealed record ModuleServiceValidationResultDto(
    bool AllValid,
    IReadOnlyList<Guid> InvalidServiceIds,
    IReadOnlyList<Guid> InactiveServiceIds
);
