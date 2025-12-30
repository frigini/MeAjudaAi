using MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Shared.Contracts.Functional;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

/// <summary>
/// Interface Refit para a API REST de Providers.
/// Define endpoints HTTP para operações CRUD e consultas de providers.
/// </summary>
/// <remarks>
/// Esta interface é usada pelo Refit para gerar automaticamente
/// o cliente HTTP tipado. Retorna Result&lt;T&gt; para tratamento
/// funcional de erros no frontend Blazor WASM.
/// </remarks>
public interface IProvidersApi
{
    /// <summary>
    /// Lista todos os providers com paginação.
    /// </summary>
    /// <param name="pageNumber">Número da página (1-based)</param>
    /// <param name="pageSize">Tamanho da página (máximo 100)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Lista paginada de providers com metadados de paginação</returns>
    /// <response code="200">Lista de providers retornada com sucesso</response>
    /// <response code="400">Parâmetros de paginação inválidos</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão para listar providers</response>
    [Get("/api/v1/providers")]
    Task<Result<PagedResult<ModuleProviderDto>>> GetProvidersAsync(
        [Query] int pageNumber = 1,
        [Query] int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um provider específico pelo ID.
    /// </summary>
    /// <param name="id">ID único do provider (GUID)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Dados completos do provider ou null se não encontrado</returns>
    /// <response code="200">Provider encontrado</response>
    /// <response code="404">Provider não encontrado</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão para visualizar este provider</response>
    [Get("/api/v1/providers/{id}")]
    Task<Result<ModuleProviderDto?>> GetProviderByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca providers por tipo (PF ou PJ).
    /// </summary>
    /// <param name="providerType">Tipo do provider: "PF" (Pessoa Física) ou "PJ" (Pessoa Jurídica)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Lista de providers do tipo especificado</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="400">Tipo de provider inválido</response>
    /// <response code="401">Não autenticado</response>
    [Get("/api/v1/providers/type/{providerType}")]
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByTypeAsync(
        string providerType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca providers por status de verificação.
    /// </summary>
    /// <param name="verificationStatus">Status: "Pending", "Verified", "Rejected"</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Lista de providers com o status especificado</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="400">Status de verificação inválido</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão para filtrar por status</response>
    [Get("/api/v1/providers/verification-status/{verificationStatus}")]
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByVerificationStatusAsync(
        string verificationStatus,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca providers por cidade.
    /// </summary>
    /// <param name="city">Nome da cidade</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Lista de providers localizados na cidade especificada</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="400">Nome de cidade inválido</response>
    /// <response code="401">Não autenticado</response>
    [Get("/api/v1/providers/city/{city}")]
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByCityAsync(
        string city,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca providers por estado (UF).
    /// </summary>
    /// <param name="state">Sigla do estado (UF) - ex: "SP", "RJ", "MG"</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Lista de providers localizados no estado especificado</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="400">Sigla de estado inválida</response>
    /// <response code="401">Não autenticado</response>
    [Get("/api/v1/providers/state/{state}")]
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByStateAsync(
        string state,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca provider pelo documento (CPF/CNPJ).
    /// </summary>
    /// <param name="document">CPF (11 dígitos) ou CNPJ (14 dígitos) apenas números</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Dados do provider ou null se não encontrado</returns>
    /// <response code="200">Provider encontrado</response>
    /// <response code="404">Provider não encontrado</response>
    /// <response code="400">Formato de documento inválido</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão para buscar por documento</response>
    [Get("/api/v1/providers/document/{document}")]
    Task<Result<ModuleProviderDto?>> GetProviderByDocumentAsync(
        string document,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Representa um resultado paginado de uma consulta à API.
/// </summary>
/// <typeparam name="T">Tipo dos itens na página</typeparam>
/// <remarks>
/// Usado para retornar listas paginadas com metadados de navegação,
/// permitindo implementar paginação no frontend de forma eficiente.
/// </remarks>
public sealed record PagedResult<T>
{
    /// <summary>
    /// Itens da página atual
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>
    /// Número da página atual (1-based)
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// Tamanho da página (quantidade de itens por página)
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Total de itens em todas as páginas
    /// </summary>
    public required int TotalItems { get; init; }

    /// <summary>
    /// Total de páginas disponíveis
    /// </summary>
    public required int TotalPages { get; init; }

    /// <summary>
    /// Indica se existe página anterior
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indica se existe próxima página
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
