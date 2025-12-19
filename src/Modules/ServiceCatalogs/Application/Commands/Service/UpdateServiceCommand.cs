using System.ComponentModel.DataAnnotations;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;

/// <summary>
/// Comando para atualizar os detalhes de um serviço existente.
/// Os limites de validação devem corresponder a ValidationConstants.CatalogLimits.
/// Nota: Validação de Guid.Empty é tratada pelo command handler para fornecer mensagens de erro específicas do domínio.
/// </summary>
public sealed record UpdateServiceCommand(
    Guid Id,
    [Required]
    [MaxLength(150)]
    string Name,
    [MaxLength(1000)]
    string? Description,
    [Range(0, int.MaxValue)]
    int DisplayOrder
) : Command<Result>;
