using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Contracts.Functional;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

/// <summary>
/// Interface Refit para a API REST de Locations.
/// Define endpoints HTTP para gestão de restrições geográficas (cidades permitidas).
/// </summary>
/// <remarks>
/// Esta interface é usada pelo Refit para gerar automaticamente
/// o cliente HTTP tipado. Retorna Result&lt;T&gt; para tratamento
/// funcional de erros no frontend Blazor WASM.
/// </remarks>
public interface ILocationsApi
{
    /// <summary>
    /// Lista todas as cidades permitidas com filtro opcional de ativas/inativas.
    /// </summary>
    /// <param name="onlyActive">Se true, retorna apenas cidades ativas. Default: true</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Lista de cidades ordenadas por estado e nome</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="401">Não autenticado</response>
    [Get("/api/v1/admin/allowed-cities")]
    Task<Result<IReadOnlyList<ModuleAllowedCityDto>>> GetAllAllowedCitiesAsync(
        [Query] bool onlyActive = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca uma cidade permitida pelo ID.
    /// </summary>
    /// <param name="id">ID da cidade (GUID)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Detalhes da cidade permitida</returns>
    /// <response code="200">Cidade encontrada</response>
    /// <response code="404">Cidade não encontrada</response>
    /// <response code="401">Não autenticado</response>
    [Get("/api/v1/admin/allowed-cities/{id}")]
    Task<Result<ModuleAllowedCityDto>> GetAllowedCityByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma nova cidade permitida (Admin only).
    /// </summary>
    /// <param name="request">Dados da cidade (nome, estado, país, lat/lng, raio de atendimento)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>ID da cidade criada</returns>
    /// <response code="201">Cidade criada com sucesso</response>
    /// <response code="400">Dados inválidos ou cidade duplicada (nome + estado)</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão de administrador</response>
    [Post("/api/v1/admin/allowed-cities")]
    Task<Result<Guid>> CreateAllowedCityAsync(
        [Body] CreateAllowedCityRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma cidade permitida existente (Admin only).
    /// </summary>
    /// <param name="id">ID da cidade (GUID)</param>
    /// <param name="request">Dados atualizados da cidade</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Confirmação da atualização</returns>
    /// <response code="200">Cidade atualizada com sucesso</response>
    /// <response code="400">Dados inválidos ou conflito com cidade existente</response>
    /// <response code="404">Cidade não encontrada</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão de administrador</response>
    [Put("/api/v1/admin/allowed-cities/{id}")]
    Task<Result<Unit>> UpdateAllowedCityAsync(
        Guid id,
        [Body] UpdateAllowedCityRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza parcialmente uma cidade permitida (Admin only).
    /// </summary>
    /// <param name="id">ID da cidade</param>
    /// <param name="request">Dados para atualização parcial</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    [Patch("/api/v1/admin/allowed-cities/{id}")]
    Task<Result<Unit>> PatchAllowedCityAsync(
        Guid id,
        [Body] PatchAllowedCityRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deleta uma cidade permitida (Admin only).
    /// </summary>
    /// <param name="id">ID da cidade (GUID)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Confirmação da exclusão</returns>
    /// <response code="204">Cidade deletada com sucesso</response>
    /// <response code="404">Cidade não encontrada</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão de administrador</response>
    [Delete("/api/v1/admin/allowed-cities/{id}")]
    Task<Result<Unit>> DeleteAllowedCityAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca cidades permitidas por estado (UF).
    /// </summary>
    /// <param name="state">Sigla do estado (ex: "SP", "RJ", "MG")</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Lista de cidades do estado especificado</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="400">Sigla de estado inválida</response>
    /// <response code="401">Não autenticado</response>
    [Get("/api/v1/admin/allowed-cities/state/{state}")]
    Task<Result<IReadOnlyList<ModuleAllowedCityDto>>> GetAllowedCitiesByStateAsync(
        string state,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca cidades cadastradas ou geolocalizadas (candidatos).
    /// </summary>
    /// <param name="query">Nome da cidade para busca</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de candidatos encontrados</returns>
    [Get("/api/v1/locations/search")]
    Task<List<LocationCandidate>> SearchAllowedCitiesAsync(
        [Query] string query,
        CancellationToken cancellationToken = default);
}
