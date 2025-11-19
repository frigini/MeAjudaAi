using MeAjudaAi.Modules.Catalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Modules.Catalogs.Domain.Exceptions;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using ServiceCategoryEntity = MeAjudaAi.Modules.Catalogs.Domain.Entities.ServiceCategory;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class CreateServiceCategoryCommandHandler(
    IServiceCategoryRepository categoryRepository)
    : ICommandHandler<CreateServiceCategoryCommand, Result<ServiceCategoryDto>>
{
    public async Task<Result<ServiceCategoryDto>> HandleAsync(CreateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedName = request.Name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                return Result<ServiceCategoryDto>.Failure("Category name is required.");

            // Verificar se já existe categoria com o mesmo nome
            if (await categoryRepository.ExistsWithNameAsync(normalizedName, null, cancellationToken))
                return Result<ServiceCategoryDto>.Failure($"A category with name '{normalizedName}' already exists.");

            var category = ServiceCategoryEntity.Create(normalizedName, request.Description, request.DisplayOrder);

            await categoryRepository.AddAsync(category, cancellationToken);

            var dto = new ServiceCategoryDto(
                category.Id.Value,
                category.Name,
                category.Description,
                category.IsActive,
                category.DisplayOrder,
                category.CreatedAt,
                category.UpdatedAt
            );

            return Result<ServiceCategoryDto>.Success(dto);
        }
        catch (CatalogDomainException ex)
        {
            return Result<ServiceCategoryDto>.Failure(ex.Message);
        }
    }
}
