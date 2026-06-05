using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class CreateServiceCategoryCommandHandler : ICommandHandler<CreateServiceCategoryCommand, Result<ServiceCategoryDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IServiceCategoryQueries _categoryQueries;
    private readonly ILogger<CreateServiceCategoryCommandHandler> _logger;

    public CreateServiceCategoryCommandHandler(
        [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
        IServiceCategoryQueries categoryQueries,
        ILogger<CreateServiceCategoryCommandHandler> logger)
    {
        _uow = uow;
        _categoryQueries = categoryQueries;
        _logger = logger;
    }

    public async Task<Result<ServiceCategoryDto>> HandleAsync(CreateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedName = request.Name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                return Result<ServiceCategoryDto>.Failure("O nome da categoria é obrigatório.");

            if (await _categoryQueries.ExistsWithNameAsync(normalizedName, null, cancellationToken))
                return Result<ServiceCategoryDto>.Failure($"Já existe uma categoria com o nome '{normalizedName}'.");

            var category = Domain.Entities.ServiceCategory.Create(normalizedName, request.Description, request.DisplayOrder);

            _uow.GetRepository<Domain.Entities.ServiceCategory, ServiceCategoryId>().Add(category);
            await _uow.SaveChangesAsync(cancellationToken);

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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (CatalogDomainException ex)
        {
            return Result<ServiceCategoryDto>.Failure(ex.Message);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while processing the request.");
            return Result<ServiceCategoryDto>.Failure("Ocorreu um erro inesperado ao processar a solicitação.");
        }
    }
}




