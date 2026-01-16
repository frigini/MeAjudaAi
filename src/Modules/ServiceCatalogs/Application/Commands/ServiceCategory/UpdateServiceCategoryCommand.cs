using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;

/// <summary>
/// Comando para atualizar as informações de uma categoria de serviço existente.
/// Nota: Este comando requer todos os campos para atualizações (padrão de atualização completa).
/// Melhoria futura: Considerar suporte a atualizações parciais onde clientes enviam apenas campos alterados
/// usando campos nullable ou tipos wrapper opcionais se os requisitos da API evoluírem.
/// </summary>
public sealed record UpdateServiceCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder = 0
) : Command<Result>;
