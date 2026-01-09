using Fluxor;
using MeAjudaAi.Shared.Contracts.Modules.Documents.DTOs;

namespace MeAjudaAi.Web.Admin.Features.Documents;

/// <summary>
/// Estado global para gestão de documentos de providers.
/// Mantém lista de documentos, filtros e estado de loading/erro.
/// </summary>
[FeatureState]
public sealed record DocumentsState
{
    /// <summary>
    /// Lista de documentos carregados
    /// </summary>
    public IReadOnlyList<ModuleDocumentDto> Documents { get; init; } = Array.Empty<ModuleDocumentDto>();

    /// <summary>
    /// ID do provider atualmente selecionado para visualização de documentos
    /// </summary>
    public Guid? SelectedProviderId { get; init; }

    /// <summary>
    /// Indica se está carregando dados
    /// </summary>
    public bool IsLoading { get; init; }

    /// <summary>
    /// Mensagem de erro caso ocorra falha
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Indica se houve erro
    /// </summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
}
