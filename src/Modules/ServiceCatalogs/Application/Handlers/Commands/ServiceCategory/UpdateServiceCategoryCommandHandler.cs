using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceCategoryEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class UpdateServiceCategoryCommandHandler(
    IUnitOfWork uow,
    IServiceCategoryQueries categoryQueries,
    ILogger<UpdateServiceCategoryCommandHandler> logger)
    : ICommandHandler<UpdateServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Id == Guid.Empty)
            {
                logger.LogWarning("UpdateServiceCategoryCommand failed: request Id is empty");
                return Result.Failure(ValidationMessages.Required.Id);
            }

            var categoryRepository = uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>();
            var category = await categoryRepository.TryFindAsync(ServiceCategoryId.From(request.Id), cancellationToken);

            if (category is null)
            {
                logger.LogWarning("UpdateServiceCategoryCommand failed: category not found. CategoryId: {CategoryId}", request.Id);
                return Result.Failure(Error.NotFound(ValidationMessages.NotFound.Category));
            }

            var normalizedName = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
                return Result.Failure(ValidationMessages.Required.CategoryName);

            if (await categoryQueries.ExistsWithNameAsync(normalizedName, category.Id, cancellationToken))
            {
                logger.LogWarning("UpdateServiceCategoryCommand failed: category name already exists. Name: {Name}", normalizedName);
                return Result.Failure(string.Format(ValidationMessages.Catalogs.CategoryNameExists, normalizedName));
            }

            category.Update(normalizedName, request.Description, request.DisplayOrder);

            try
            {
                await uow.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                if (IsUniqueConstraintViolation(ex))
                {
                    return Result.Failure(string.Format(ValidationMessages.Catalogs.CategoryNameExists, normalizedName));
                }
                logger.LogError(ex, "Database error updating category {CategoryId}", request.Id);
                throw;
            }

            return Result.Success();
        }
        catch (CatalogDomainException ex)
        {
            logger.LogError(ex, "Domain error updating category {CategoryId}", request.Id);
            return Result.Failure(ex.Message);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("unique") == true ||
               ex.InnerException?.Message.Contains("duplicate key") == true ||
               ex.InnerException?.Message.Contains("ix_service_categories_name") == true;
    }
}