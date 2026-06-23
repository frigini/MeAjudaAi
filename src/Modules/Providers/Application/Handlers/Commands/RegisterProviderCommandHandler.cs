using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler para registro de novos providers no sistema.
/// </summary>
public sealed class RegisterProviderCommandHandler(
    IUnitOfWork uow,
    IProviderQueries providerQueries,
    ILogger<RegisterProviderCommandHandler> logger
) : ICommandHandler<RegisterProviderCommand, Result<ProviderDto>>
{
    public async Task<Result<ProviderDto>> HandleAsync(RegisterProviderCommand command, CancellationToken cancellationToken)
    {
        var existingProvider = await providerQueries.GetByUserIdAsync(command.UserId, cancellationToken);
        if (existingProvider is not null)
        {
            return Result<ProviderDto>.Success(existingProvider.ToDto());
        }

        try
        {
            var contactInfo = new ContactInfo(command.Email, command.PhoneNumber);
            
            var address = new Address(
                "Rua Pendente",
                "0",
                "Bairro Pendente",
                "Cidade Pendente",
                "SP",
                "00000-000",
                "Brasil"
            ); 
            
            var businessProfile = new BusinessProfile(
                command.Name,
                contactInfo,
                address,
                command.Name,
                "Prestador de serviços"
            );

            var provider = new Provider(
                command.UserId,
                command.Name,
                command.Type,
                businessProfile
            );

            var docType = (command.Type == EProviderType.Individual || command.Type == EProviderType.Freelancer) 
                ? EDocumentType.CPF 
                : EDocumentType.CNPJ;
            var doc = new Document(command.DocumentNumber, docType, isPrimary: true);
            provider.AddDocument(doc);
            
            uow.GetRepository<Provider, ProviderId>().Add(provider);
            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Provider {ProviderId} created successfully via registration for user {UserId}",
                provider.Id.Value, command.UserId);

            return Result<ProviderDto>.Success(provider.ToDto());
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
            {
                logger.LogWarning(ex, "Uniqueness violation in RegisterProvider for user {UserId}. Checking if provider already exists.", command.UserId);
                
                // Recalcula se o prestador já existe (race condition entre a verificação inicial e o save)
                var duplicateProvider = await providerQueries.GetByUserIdAsync(command.UserId, cancellationToken);
                if (duplicateProvider is not null)
                {
                    return Result<ProviderDto>.Success(duplicateProvider.ToDto());
                }
            }

            logger.LogError(ex, "Database update error in RegisterProvider for user {UserId}", command.UserId);
            return Result<ProviderDto>.Failure(new Error("Erro ao salvar os dados. Verifique se as informações já estão cadastradas ou tente novamente.", 500));
        }
        catch (Exception ex) when (ex is DomainException || ex is ArgumentException)
        {
            logger.LogWarning(ex, "Validation error in RegisterProvider for user {UserId}: {Message}", command.UserId, ex.Message);
            return Result<ProviderDto>.Failure(new Error("Erro ao processar a requisição. Verifique os dados informados.", 400));
        }
    }
}