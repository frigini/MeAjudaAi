using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

/// <summary>
/// Handler para o comando CreateServiceCategoryCommand, responsável por criar uma nova categoria de serviço no catálogo.
/// </summary>
/// <param name="uow"></param>
/// <param name="categoryQueries"></param>
/// <param name="logger"></param>
public sealed class CreateServiceCategoryCommandHandler(
    [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
    IServiceCategoryQueries categoryQueries,
    ILogger<CreateServiceCategoryCommandHandler> logger) : ICommandHandler<CreateServiceCategoryCommand, Result<ServiceCategoryDto>>
{
    public async Task<Result<ServiceCategoryDto>> HandleAsync(CreateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedName = request.Name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                return Result<ServiceCategoryDto>.Failure("O nome da categoria é obrigatório.");

            if (await categoryQueries.ExistsWithNameAsync(normalizedName, null, cancellationToken))
                return Result<ServiceCategoryDto>.Failure($"Já existe uma categoria com o nome '{normalizedName}'.");

            var category = Domain.Entities.ServiceCategory.Create(normalizedName, request.Description, request.DisplayOrder);

            uow.GetRepository<Domain.Entities.ServiceCategory, ServiceCategoryId>().Add(category);
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
            logger.LogError(ex, "Unexpected error while creating service category.");
            return Result<ServiceCategoryDto>.Failure("Ocorreu um erro inesperado ao processar a solicitação.");
        }
    }
}