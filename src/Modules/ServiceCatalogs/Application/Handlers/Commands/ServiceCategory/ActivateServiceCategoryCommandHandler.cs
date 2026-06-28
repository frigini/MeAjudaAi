using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

/// <summary>
/// Handler para o comando ActivateServiceCategoryCommand, responsável por ativar uma categoria de serviço existente.
/// </summary>
/// <param name="uow"></param>
/// <param name="logger"></param>
public sealed class ActivateServiceCategoryCommandHandler(
    [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
    ILogger<ActivateServiceCategoryCommandHandler> logger) : ICommandHandler<ActivateServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(ActivateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Id == Guid.Empty)
                return Result.Failure("O ID da categoria não pode ser vazio.");

            var categoryId = ServiceCategoryId.From(request.Id);
            var category = await uow.GetRepository<Domain.Entities.ServiceCategory, ServiceCategoryId>().TryFindAsync(categoryId, cancellationToken);

            if (category is null)
                return Result.Failure($"Categoria com ID '{request.Id}' não encontrada.");

            category.Activate();

            await uow.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while activating service category.");
            return Result.Failure("Ocorreu um erro inesperado ao ativar a categoria de serviço.");
        }
    }
}
