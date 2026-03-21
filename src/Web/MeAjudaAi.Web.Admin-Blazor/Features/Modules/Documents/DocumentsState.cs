using Fluxor;
using MeAjudaAi.Contracts.Modules.Documents.DTOs;

namespace MeAjudaAi.Web.Admin.Features.Modules.Documents;

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

    /// <summary>
    /// Indica se está excluindo um documento
    /// </summary>
    public bool IsDeleting { get; init; }

    /// <summary>
    /// ID do documento sendo excluído
    /// </summary>
    public Guid? DeletingDocumentId { get; init; }

    /// <summary>
    /// Indica se está solicitando verificação
    /// </summary>
    public bool IsRequestingVerification { get; init; }

    /// <summary>
    /// ID do documento sendo verificado
    /// </summary>
    public Guid? VerifyingDocumentId { get; init; }
}
