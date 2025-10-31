using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Services;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar queries de listagem paginada de prestadores de serviços.
/// </summary>
/// <remarks>
/// Implementa busca paginada com filtros opcionais aplicando regras de negócio:
/// - Exclui prestadores com soft delete (IsDeleted = true)
/// - Aplica filtros de busca por nome (case-insensitive)
/// - Filtra por tipo e status de verificação quando especificados
/// - Ordena resultados por data de criação (mais recentes primeiro)
/// </remarks>
public class GetProvidersQueryHandler(
    IProviderQueryService providerQueryService,
    ILogger<GetProvidersQueryHandler> logger) : IQueryHandler<GetProvidersQuery, Result<PagedResult<ProviderDto>>>
{

    /// <summary>
    /// Processa a query de listagem de prestadores aplicando filtros e paginação.
    /// </summary>
    /// <param name="query">Query com parâmetros de busca</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado paginado com lista de prestadores</returns>
    public async Task<Result<PagedResult<ProviderDto>>> HandleAsync(
        GetProvidersQuery query, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Processando busca de prestadores - Página: {Page}, Tamanho: {PageSize}, Filtros: Nome='{Name}', Tipo={Type}, Status={Status}",
                query.Page, query.PageSize, query.Name, query.Type, query.VerificationStatus);

            // Aplica filtros na busca
            var providers = await providerQueryService.GetProvidersAsync(
                page: query.Page,
                pageSize: query.PageSize,
                nameFilter: query.Name,
                typeFilter: query.Type.HasValue ? (EProviderType)query.Type.Value : null,
                verificationStatusFilter: query.VerificationStatus.HasValue ? (EVerificationStatus)query.VerificationStatus.Value : null,
                cancellationToken: cancellationToken);

            // Converte para DTOs
            var providerDtos = providers.Items.Select(provider => new ProviderDto(
                Id: provider.Id.Value,
                UserId: provider.UserId,
                Name: provider.Name,
                Type: provider.Type,
                BusinessProfile: new BusinessProfileDto(
                    LegalName: provider.BusinessProfile.LegalName,
                    FantasyName: provider.BusinessProfile.FantasyName,
                    Description: provider.BusinessProfile.Description,
                    ContactInfo: new ContactInfoDto(
                        Email: provider.BusinessProfile.ContactInfo.Email,
                        PhoneNumber: provider.BusinessProfile.ContactInfo.PhoneNumber,
                        Website: provider.BusinessProfile.ContactInfo.Website
                    ),
                    PrimaryAddress: new AddressDto(
                        Street: provider.BusinessProfile.PrimaryAddress.Street,
                        Number: provider.BusinessProfile.PrimaryAddress.Number,
                        Complement: provider.BusinessProfile.PrimaryAddress.Complement,
                        Neighborhood: provider.BusinessProfile.PrimaryAddress.Neighborhood,
                        City: provider.BusinessProfile.PrimaryAddress.City,
                        State: provider.BusinessProfile.PrimaryAddress.State,
                        ZipCode: provider.BusinessProfile.PrimaryAddress.ZipCode,
                        Country: provider.BusinessProfile.PrimaryAddress.Country
                    )
                ),
                VerificationStatus: provider.VerificationStatus,
                Documents: provider.Documents.Select(d => new DocumentDto(
                    Number: d.Number,
                    DocumentType: d.DocumentType
                )).ToList(),
                Qualifications: provider.Qualifications.Select(q => new QualificationDto(
                    Name: q.Name,
                    Description: q.Description,
                    IssuingOrganization: q.IssuingOrganization,
                    IssueDate: q.IssueDate,
                    ExpirationDate: q.ExpirationDate,
                    DocumentNumber: q.DocumentNumber
                )).ToList(),
                CreatedAt: provider.CreatedAt,
                UpdatedAt: provider.UpdatedAt,
                IsDeleted: provider.IsDeleted,
                DeletedAt: provider.DeletedAt
            )).ToList();

            var result = new PagedResult<ProviderDto>(
                providerDtos,
                providers.TotalCount,
                query.Page,
                query.PageSize);

            logger.LogInformation(
                "Busca de prestadores concluída - Total: {Total}, Página atual: {Page}/{TotalPages}",
                result.TotalCount, result.Page, result.TotalPages);

            return Result<PagedResult<ProviderDto>>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, 
                "Erro ao buscar prestadores - Página: {Page}, Filtros: Nome='{Name}', Tipo={Type}, Status={Status}",
                query.Page, query.Name, query.Type, query.VerificationStatus);

            return Result<PagedResult<ProviderDto>>.Failure(Error.Internal(
                "Erro interno ao buscar prestadores"));
        }
    }
}