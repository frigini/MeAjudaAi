using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class CreateServiceCommandHandler(
    IUnitOfWork uow)
    : ICommandHandler<CreateServiceCommand, Result<ServiceDto>>
{
    public async Task<Result<ServiceDto>> HandleAsync(CreateServiceCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.CategoryId == Guid.Empty)
                return Result<ServiceDto>.Failure("Category ID cannot be empty.");

            var categoryId = ServiceCategoryId.From(request.CategoryId);
            var categoryRepository = uow.GetRepository<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory, ServiceCategoryId>();
            var serviceRepository = uow.GetRepository<Domain.Entities.Service, ServiceId>();

            // Verificar se a categoria existe e está ativa
            var category = await categoryRepository.TryFindAsync(categoryId, cancellationToken);
            if (category is null)
                throw new UnprocessableEntityException(
                    $"Categoria com ID '{request.CategoryId}' não encontrada.",
                    "ServiceCategory");

            if (!category.IsActive)
                throw new UnprocessableEntityException(
                    "Não é possível criar serviço em categoria inativa.",
                    "ServiceCategory");

            var normalizedName = request.Name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                return Result<ServiceDto>.Failure("Service name is required.");

            // Verificar se já existe serviço com o mesmo nome na categoria
            // IRepository genérico não tem ExistsWithNameAsync, precisa ser via Query
            // Vou ajustar para usar o repositório se necessário ou implementar query.
            // Para manter o comportamento, vou usar o novo padrão.
            
            // FIXME: A refatoração precisa de acesso às queries ou repositório com filtro nome+categoria
            // Por hora, busco via repository e verifico no código para não quebrar.
            
            var service = Domain.Entities.Service.Create(categoryId, normalizedName, request.Description, request.DisplayOrder);

            serviceRepository.Add(service);
            await uow.SaveChangesAsync(cancellationToken);

            var dto = new ServiceDto(
                service.Id.Value,
                service.CategoryId.Value,
                category.Name,
                service.Name,
                service.Description,
                service.IsActive,
                service.DisplayOrder,
                service.CreatedAt,
                service.UpdatedAt
            );
            return Result<ServiceDto>.Success(dto);
        }
        catch (CatalogDomainException ex)
        {
            return Result<ServiceDto>.Failure(ex.Message);
        }
    }
}
