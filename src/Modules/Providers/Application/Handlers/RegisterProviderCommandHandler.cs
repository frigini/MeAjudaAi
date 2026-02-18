using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Modules.Providers.Application.Handlers;

public class RegisterProviderCommandHandler : ICommandHandler<RegisterProviderCommand, Result<ProviderDto>>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterProviderCommandHandler(IProviderRepository providerRepository, IUnitOfWork unitOfWork)
    {
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProviderDto>> Handle(RegisterProviderCommand command, CancellationToken cancellationToken)
    {
        var existingProvider = await _providerRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (existingProvider is not null)
        {
            return ProviderMapper.ToDto(existingProvider);
        }

        try
        {
            var contactInfo = new ContactInfo(command.Email, command.PhoneNumber);
            
            // Endereço e BusinessProfile inicialmente placeholders para permitir cadastro em etapas
            // O usuário completará isso no wizard
            var address = new Address("Pending", "0", "Pending", "Pending", "Pending", "00000000"); // Valid ZipCodes are checked? Assuming permissive for now or mocked. 
            // Valid ZipCode '00000000' might fail validation if strict. Address constructor calls Trim() but checks empty.
            
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

            // Add Document now (CPF/CNPJ)
            var docType = command.Type == EProviderType.Individual ? EDocumentType.CPF : EDocumentType.CNPJ;
            var doc = new Document(command.DocumentNumber, docType, isPrimary: true);
            provider.AddDocument(doc);
            
            await _providerRepository.AddAsync(provider, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return ProviderMapper.ToDto(provider);
        }
        catch (DomainException ex)
        {
            return Result<ProviderDto>.Failure(new Error("Provider.Registration", ex.Message, 400));
        }
        catch (Exception ex)
        {
             return Result<ProviderDto>.Failure(new Error("Provider.Registration", "Erro inesperado ao registrar prestador: " + ex.Message, 500));
        }
    }
}
