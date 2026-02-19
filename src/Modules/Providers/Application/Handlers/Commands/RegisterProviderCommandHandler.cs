using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Database.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

public sealed class RegisterProviderCommandHandler(
    IProviderRepository providerRepository,
    ILogger<RegisterProviderCommandHandler> logger
) : ICommandHandler<RegisterProviderCommand, Result<ProviderDto>>
{
    public async Task<Result<ProviderDto>> HandleAsync(RegisterProviderCommand command, CancellationToken cancellationToken)
    {
        var existingProvider = await providerRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (existingProvider is not null)
        {
            return Result<ProviderDto>.Success(existingProvider.ToDto());
        }

        try
        {
            var contactInfo = new ContactInfo(command.Email, command.PhoneNumber);
            
            // Endereço e BusinessProfile inicialmente placeholders para permitir cadastro em etapas
            // Usando valores sentinela claros para indicar pendência
            var address = new Address("Pending", "0", "Pending", "XX", "00000-000", "00000000"); 
            
            var businessProfile = new BusinessProfile(
                command.Name, // LegalName
                contactInfo,
                address,
                command.Name, // FantasyName default
                "Prestador de serviços" // Description default
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
            
            await providerRepository.AddAsync(provider, cancellationToken);

            logger.LogInformation("Provider {ProviderId} created successfully via registration for user {UserId}",
                provider.Id.Value, command.UserId);

            return Result<ProviderDto>.Success(provider.ToDto());
        }
        catch (DbUpdateException ex)
        {
            var processedEx = PostgreSqlExceptionProcessor.ProcessException(ex);
            
            if (processedEx is UniqueConstraintException)
            {
                logger.LogWarning(ex, "Duplicate provider registration attempt for user {UserId}", command.UserId);
                var existing = await providerRepository.GetByUserIdAsync(command.UserId, cancellationToken);
                if (existing is not null)
                {
                    return Result<ProviderDto>.Success(existing.ToDto());
                }
                
                // Se houver violação de constraint única mas não encontrarmos o prestador,
                // isso implica numa condição de corrida ou inconsistência de dados que devemos reportar como falha
                return Result<ProviderDto>.Failure(Error.Conflict("Um prestador já está registrado para este usuário."));
            }
            // Para outros erros de banco de dados, relança para ser tratado pelo handler global de exceções ou catch externo
            throw processedEx;
        }
        catch (Exception ex) when (ex is DomainException || ex is ArgumentException)
        {
            logger.LogWarning(ex, "Validation error in RegisterProvider for user {UserId}: {Message}", command.UserId, ex.Message);
            return Result<ProviderDto>.Failure(new Error("Erro ao processar a requisição. Verifique os dados informados.", 400));
        }
        catch (Exception ex)
        {
             logger.LogError(ex, "Error handling RegisterProviderCommand for user {UserId}", command.UserId);
             return Result<ProviderDto>.Failure(new Error("Erro inesperado ao registrar prestador.", 500));
        }
    }
}
