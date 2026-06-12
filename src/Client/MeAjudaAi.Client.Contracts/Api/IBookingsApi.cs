using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

/// <summary>
/// Interface Refit para a API REST de Bookings.
/// Define endpoints HTTP para criação e consulta de agendamentos de serviços.
/// </summary>
/// <remarks>
/// Esta interface é usada pelo Refit para gerar automaticamente
/// o cliente HTTP tipado. Os DTOs são compartilhados de MeAjudaAi.Contracts.Modules.Bookings.DTOs.
/// </remarks>
public interface IBookingsApi
{
    /// <summary>
    /// Cria um novo agendamento de serviço.
    /// </summary>
    /// <param name="request">Dados do agendamento (ProviderId, ServiceId, Start, End)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Agendamento criado com dados completos</returns>
    /// <response code="201">Agendamento criado com sucesso</response>
    /// <response code="400">Dados inválidos ou conflito de horário</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="409">Conflito de agendamento (horário indisponível)</response>
    [Post("/api/v1/bookings")]
    Task<ModuleBookingDto> CreateBookingAsync(
        [Body] CreateBookingRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera um agendamento pelo ID.
    /// </summary>
    /// <param name="id">Identificador único do agendamento (GUID)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Dados completos do agendamento</returns>
    /// <response code="200">Agendamento encontrado</response>
    /// <response code="404">Agendamento não encontrado</response>
    /// <response code="401">Não autenticado</response>
    [Get("/api/v1/bookings/{id}")]
    Task<ModuleBookingDto> GetBookingByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
