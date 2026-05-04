using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using ServiceEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class UpdateServiceCommandHandler(
    IUnitOfWork uow,
    IServiceQueries serviceQueries)
    : ICommandHandler<UpdateServiceCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateServiceCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Id == Guid.Empty)
                return Result.Failure(ValidationMessages.Required.Id);

            var serviceRepository = uow.GetRepository<ServiceEntity, ServiceId>();
            var service = await serviceRepository.TryFindAsync(ServiceId.From(request.Id), cancellationToken);

            if (service is null)
                return Result.Failure(Error.NotFound(ValidationMessages.NotFound.Service));

            var normalizedName = request.Name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                return Result.Failure(ValidationMessages.Required.ServiceName);

            if (await serviceQueries.ExistsWithNameAsync(normalizedName, service.Id, service.CategoryId, cancellationToken))
                return Result.Failure(string.Format(ValidationMessages.Catalogs.ServiceNameExists, normalizedName));

            service.Update(normalizedName, request.Description, request.DisplayOrder);

            await uow.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (CatalogDomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}