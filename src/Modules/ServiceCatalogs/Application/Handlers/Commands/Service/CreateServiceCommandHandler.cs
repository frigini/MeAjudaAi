using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class CreateServiceCommandHandler : ICommandHandler<CreateServiceCommand, Result<ServiceDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IServiceQueries _serviceQueries;
    private readonly IServiceCategoryQueries _categoryQueries;

    public CreateServiceCommandHandler(
        [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
        IServiceQueries serviceQueries,
        IServiceCategoryQueries categoryQueries)
    {
        _uow = uow;
        _serviceQueries = serviceQueries;
        _categoryQueries = categoryQueries;
    }

    public async Task<Result<ServiceDto>> HandleAsync(CreateServiceCommand request, CancellationToken cancellationToken = default)
    {
        var uow = _uow;
        var serviceQueries = _serviceQueries;
        var categoryQueries = _categoryQueries;

        try
        {
            if (request.CategoryId == Guid.Empty)
                return Result<ServiceDto>.Failure("Category ID cannot be empty.");

            var categoryId = ServiceCategoryId.From(request.CategoryId);

            // Verificar se a categoria existe e está ativa
            var category = await categoryQueries.GetByIdAsync(categoryId, cancellationToken);
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
            if (await serviceQueries.ExistsWithNameAsync(normalizedName, null, categoryId, cancellationToken))
                return Result<ServiceDto>.Failure($"A service with name '{normalizedName}' already exists in this category.");

            // Validar DisplayOrder
            if (request.DisplayOrder < 0)
                return Result<ServiceDto>.Failure("Display order cannot be negative.");

            var service = Domain.Entities.Service.Create(categoryId, normalizedName, request.Description, request.DisplayOrder);

            uow.GetRepository<Domain.Entities.Service, ServiceId>().Add(service);
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
        catch (Exception ex)
        {
            return Result<ServiceDto>.Failure($"UNEXPECTED: {ex.Message}");
        }
    }
}
