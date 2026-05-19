using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class ChangeServiceCategoryCommandHandler(
    IUnitOfWork uow,
    IServiceQueries serviceQueries,
    IServiceCategoryQueries categoryQueries)
    : ICommandHandler<ChangeServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(ChangeServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.ServiceId == Guid.Empty)
                return Result.Failure("O ID do serviço não pode ser vazio.");

            if (request.NewCategoryId == Guid.Empty)
                return Result.Failure("O ID da nova categoria não pode ser vazio.");

            var serviceId = ServiceId.From(request.ServiceId);
            var service = await uow.GetRepository<Domain.Entities.Service, ServiceId>().TryFindAsync(serviceId, cancellationToken);

            if (service is null)
                return Result.Failure(Error.NotFound($"Serviço com ID '{request.ServiceId}' não encontrado."));

            var newCategoryId = ServiceCategoryId.From(request.NewCategoryId);
            var newCategory = await categoryQueries.GetByIdAsync(newCategoryId, cancellationToken);

            if (newCategory is null)
                throw new UnprocessableEntityException(
                    $"Categoria com ID '{request.NewCategoryId}' não encontrada.",
                    "ServiceCategory");

            if (!newCategory.IsActive)
                throw new UnprocessableEntityException(
                    "Não é possível mover serviço para categoria inativa.",
                    "ServiceCategory");

            // Garantir que o nome ainda é único na categoria de destino
            if (await serviceQueries.ExistsWithNameAsync(
                    service.Name,
                    service.Id,
                    newCategoryId,
                    cancellationToken))
            {
                return Result.Failure(
                    $"Já existe um serviço com o nome '{service.Name}' na categoria de destino.");
            }

            service.ChangeCategory(newCategoryId);

            await uow.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (CatalogDomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
