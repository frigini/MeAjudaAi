using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.DependencyInjection;
using ServiceCategoryEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class CreateServiceCategoryCommandHandler : ICommandHandler<CreateServiceCategoryCommand, Result<ServiceCategoryDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IServiceCategoryQueries _categoryQueries;

    public CreateServiceCategoryCommandHandler(
        [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
        IServiceCategoryQueries categoryQueries)
    {
        _uow = uow;
        _categoryQueries = categoryQueries;
    }

    public async Task<Result<ServiceCategoryDto>> HandleAsync(CreateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        var uow = _uow;
        var categoryQueries = _categoryQueries;
        try
        {
            var normalizedName = request.Name?.Trim();


            if (string.IsNullOrWhiteSpace(normalizedName))
                return Result<ServiceCategoryDto>.Failure("Category name is required.");

            // Verificar se já existe categoria com o mesmo nome
            if (await categoryQueries.ExistsWithNameAsync(normalizedName, null, cancellationToken))
                return Result<ServiceCategoryDto>.Failure($"A category with name '{normalizedName}' already exists.");

            var category = ServiceCategoryEntity.Create(normalizedName, request.Description, request.DisplayOrder);

            uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>().Add(category);
            await uow.SaveChangesAsync(cancellationToken);

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
        catch (Exception ex)
        {
            return Result<ServiceCategoryDto>.Failure($"UNEXPECTED: {ex.Message} | Stack: {ex.StackTrace}");
        }
    }
}
