using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

/// <summary>
/// Handler para o comando ChangeServiceCategoryCommand, responsável por alterar a categoria de um serviço existente.
/// </summary>
/// <param name="uow"></param>
/// <param name="serviceQueries"></param>
/// <param name="categoryQueries"></param>
/// <param name="logger"></param>
public sealed class ChangeServiceCategoryCommandHandler(
    [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
    IServiceQueries serviceQueries,
    IServiceCategoryQueries categoryQueries,
    ILogger<ChangeServiceCategoryCommandHandler> logger) : ICommandHandler<ChangeServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(ChangeServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.ServiceId == Guid.Empty)
                throw new UnprocessableEntityException("O ID do serviço não pode ser vazio.", "ServiceId");

            if (request.NewCategoryId == Guid.Empty)
                throw new UnprocessableEntityException("O ID da nova categoria não pode ser vazio.", "NewCategoryId");

            var serviceId = ServiceId.From(request.ServiceId);
            var service = await uow.GetRepository<Domain.Entities.Service, ServiceId>().TryFindAsync(serviceId, cancellationToken);

            if (service is null)
                return Result.Failure(Error.NotFound($"Serviço com ID '{request.ServiceId}' não encontrado."));

            var newCategoryId = ServiceCategoryId.From(request.NewCategoryId);
            var newCategory = await categoryQueries.GetByIdAsync(newCategoryId, cancellationToken) ?? throw new UnprocessableEntityException(
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (CatalogDomainException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (UnprocessableEntityException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu um erro inesperado ao alterar a categoria do serviço.");
            return Result.Failure("Ocorreu um erro inesperado ao alterar a categoria do serviço.");
        }
    }
}