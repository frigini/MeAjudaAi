using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.API.Mappers;

/// <summary>
/// Métodos de extensão para mapear DTOs para Commands e Queries do módulo Providers.
/// </summary>
public static class RequestMapperExtensions
{
    /// <summary>
    /// Mapeia CreateProviderRequest para CreateProviderCommand.
    /// </summary>
    /// <param name="request">Requisição de criação de prestador</param>
    /// <returns>CreateProviderCommand com propriedades mapeadas</returns>
    /// <remarks>
    /// ArgumentException aqui captura erros de programação para propriedades obrigatórias.
    /// FormatException pode ser lançada por Guid.Parse se UserId contém uma string GUID inválida.
    /// A validação de entrada do usuário deve ser feita via FluentValidation antes de chegar neste ponto.
    /// </remarks>
    public static CreateProviderCommand ToCommand(this CreateProviderRequest request)
    {
        return new CreateProviderCommand(
            Guid.Parse(request.UserId ?? throw new ArgumentException("UserId is required", nameof(request))),
            request.Name,
            request.Type,
            request.BusinessProfile ?? throw new ArgumentException("BusinessProfile is required", nameof(request))
        );
    }

    /// <summary>
    /// Mapeia UpdateProviderProfileRequest para UpdateProviderProfileCommand.
    /// </summary>
    /// <param name="request">Requisição de atualização de perfil</param>
    /// <param name="providerId">ID do prestador a ser atualizado</param>
    /// <returns>UpdateProviderProfileCommand com propriedades mapeadas</returns>
    public static UpdateProviderProfileCommand ToCommand(this UpdateProviderProfileRequest request, Guid providerId)
    {
        return new UpdateProviderProfileCommand(
            providerId,
            request.Name,
            request.BusinessProfile
        );
    }

    /// <summary>
    /// Mapeia AddDocumentRequest para AddDocumentCommand.
    /// </summary>
    /// <param name="request">Requisição de adição de documento</param>
    /// <param name="providerId">ID do prestador</param>
    /// <returns>AddDocumentCommand com propriedades mapeadas</returns>
    public static AddDocumentCommand ToCommand(this AddDocumentRequest request, Guid providerId)
    {
        return new AddDocumentCommand(
            providerId,
            request.Number,
            request.DocumentType
        );
    }

    /// <summary>
    /// Mapeia UpdateVerificationStatusRequest para UpdateVerificationStatusCommand.
    /// </summary>
    /// <param name="request">Requisição de atualização de status</param>
    /// <param name="providerId">ID do prestador</param>
    /// <returns>UpdateVerificationStatusCommand com propriedades mapeadas</returns>
    public static UpdateVerificationStatusCommand ToCommand(this UpdateVerificationStatusRequest request, Guid providerId)
    {
        return new UpdateVerificationStatusCommand(
            providerId,
            request.Status
        );
    }

    /// <summary>
    /// Mapeia o ID do prestador para GetProviderByIdQuery.
    /// </summary>
    /// <param name="providerId">ID do prestador a ser consultado</param>
    /// <returns>GetProviderByIdQuery com o ID especificado</returns>
    public static GetProviderByIdQuery ToQuery(this Guid providerId)
    {
        return new GetProviderByIdQuery(providerId);
    }

    /// <summary>
    /// Mapeia o ID do usuário para GetProviderByUserIdQuery.
    /// </summary>
    /// <param name="userId">ID do usuário a ser consultado</param>
    /// <returns>GetProviderByUserIdQuery com o ID especificado</returns>
    public static GetProviderByUserIdQuery ToUserQuery(this Guid userId)
    {
        return new GetProviderByUserIdQuery(userId);
    }

    /// <summary>
    /// Mapeia a cidade para GetProvidersByCityQuery.
    /// </summary>
    /// <param name="city">Nome da cidade</param>
    /// <returns>GetProvidersByCityQuery com a cidade especificada</returns>
    public static GetProvidersByCityQuery ToCityQuery(this string city)
    {
        return new GetProvidersByCityQuery(city);
    }

    /// <summary>
    /// Mapeia o estado para GetProvidersByStateQuery.
    /// </summary>
    /// <param name="state">Nome do estado</param>
    /// <returns>GetProvidersByStateQuery com o estado especificado</returns>
    public static GetProvidersByStateQuery ToStateQuery(this string state)
    {
        return new GetProvidersByStateQuery(state);
    }

    /// <summary>
    /// Mapeia o tipo para GetProvidersByTypeQuery.
    /// </summary>
    /// <param name="type">Tipo do prestador</param>
    /// <returns>GetProvidersByTypeQuery com o tipo especificado</returns>
    public static GetProvidersByTypeQuery ToTypeQuery(this EProviderType type)
    {
        return new GetProvidersByTypeQuery(type);
    }

    /// <summary>
    /// Mapeia o status de verificação para GetProvidersByVerificationStatusQuery.
    /// </summary>
    /// <param name="status">Status de verificação</param>
    /// <returns>GetProvidersByVerificationStatusQuery com o status especificado</returns>
    public static GetProvidersByVerificationStatusQuery ToVerificationStatusQuery(this EVerificationStatus status)
    {
        return new GetProvidersByVerificationStatusQuery(status);
    }

    /// <summary>
    /// Mapeia o ID do prestador para DeleteProviderCommand.
    /// </summary>
    /// <param name="providerId">ID do prestador a ser excluído</param>
    /// <returns>DeleteProviderCommand com o ID especificado</returns>
    public static DeleteProviderCommand ToDeleteCommand(this Guid providerId)
    {
        return new DeleteProviderCommand(providerId);
    }

    /// <summary>
    /// Mapeia parâmetros para RemoveDocumentCommand.
    /// </summary>
    /// <param name="providerId">ID do prestador</param>
    /// <param name="documentType">Tipo do documento a ser removido</param>
    /// <returns>RemoveDocumentCommand com os parâmetros especificados</returns>
    public static RemoveDocumentCommand ToRemoveDocumentCommand(this Guid providerId, EDocumentType documentType)
    {
        return new RemoveDocumentCommand(providerId, documentType);
    }

    /// <summary>
    /// Mapeia GetProvidersRequest para GetProvidersQuery.
    /// </summary>
    /// <param name="request">Requisição de listagem de prestadores</param>
    /// <returns>GetProvidersQuery com os parâmetros especificados</returns>
    public static GetProvidersQuery ToProvidersQuery(this GetProvidersRequest request)
    {
        return new GetProvidersQuery(
            Page: request.PageNumber,
            PageSize: request.PageSize,
            Name: request.Name,
            Type: request.Type,
            VerificationStatus: request.VerificationStatus
        );
    }
}
