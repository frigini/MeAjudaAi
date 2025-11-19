using MeAjudaAi.Modules.Catalogs.Application.Commands.Service;
using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.Exceptions;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands.Service;

public sealed class CreateServiceCommandHandler(
    IServiceRepository serviceRepository,
    IServiceCategoryRepository categoryRepository)
    : ICommandHandler<CreateServiceCommand, Result<ServiceDto>>
{
    public async Task<Result<ServiceDto>> HandleAsync(CreateServiceCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.CategoryId == Guid.Empty)
                return Result<ServiceDto>.Failure("Category ID cannot be empty.");

            var categoryId = ServiceCategoryId.From(request.CategoryId);

            // Verificar se a categoria existe e está ativa
            var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category is null)
                return Result<ServiceDto>.Failure($"Category with ID '{request.CategoryId}' not found.");

            if (!category.IsActive)
                return Result<ServiceDto>.Failure("Cannot create service in inactive category.");

            // Verificar se já existe serviço com o mesmo nome na categoria
            if (await serviceRepository.ExistsWithNameAsync(request.Name, null, categoryId, cancellationToken))
                return Result<ServiceDto>.Failure($"A service with name '{request.Name}' already exists in this category.");

            var service = Domain.Entities.Service.Create(categoryId, request.Name, request.Description, request.DisplayOrder);

            await serviceRepository.AddAsync(service, cancellationToken);

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
