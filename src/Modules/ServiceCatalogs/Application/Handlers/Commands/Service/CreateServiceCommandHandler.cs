using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

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
            if (await serviceRepository.ExistsWithNameAsync(normalizedName, null, categoryId, cancellationToken))
                return Result<ServiceDto>.Failure($"A service with name '{normalizedName}' already exists in this category.");

            // Validar DisplayOrder
            if (request.DisplayOrder < 0)
                return Result<ServiceDto>.Failure("Display order cannot be negative.");

            var service = Domain.Entities.Service.Create(categoryId, normalizedName, request.Description, request.DisplayOrder);

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
