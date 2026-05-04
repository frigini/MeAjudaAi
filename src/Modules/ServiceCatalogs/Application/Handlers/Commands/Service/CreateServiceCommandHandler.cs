using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class CreateServiceCommandHandler(
    IUnitOfWork uow, 
    IServiceCategoryQueries categoryQueries,
    IServiceQueries serviceQueries)
    : ICommandHandler<CreateServiceCommand, Result<ServiceDto>>
{   
    public async Task<Result<ServiceDto>> HandleAsync(CreateServiceCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.CategoryId == Guid.Empty)
                return Result<ServiceDto>.Failure(ValidationMessages.Required.Id);

            var categoryId = ServiceCategoryId.From(request.CategoryId);
            var category = await categoryQueries.GetByIdAsync(categoryId, cancellationToken); 
            
            if (category is null)
                return Result<ServiceDto>.Failure(Error.Unprocessable(
                    string.Format(ValidationMessages.NotFound.CategoryById, request.CategoryId),
                    "ServiceCategory"));

            if (!category.IsActive)
                return Result<ServiceDto>.Failure(Error.Unprocessable(
                    "Não é possível criar serviço em categoria inativa.",
                    "ServiceCategory"));

            var normalizedName = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
                return Result<ServiceDto>.Failure(ValidationMessages.Required.ServiceName);

            if (await serviceQueries.ExistsWithNameAsync(normalizedName, null, categoryId, cancellationToken))
                return Result<ServiceDto>.Failure(string.Format(ValidationMessages.Catalogs.ServiceNameExists, normalizedName));

            var service = Domain.Entities.Service.Create(categoryId, normalizedName, request.Description, request.DisplayOrder);

            var serviceRepo = uow.GetRepository<Domain.Entities.Service, ServiceId>();
            serviceRepo.Add(service);
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Result<ServiceDto>.Failure("Erro ao criar o serviço.");
        }
    }
}