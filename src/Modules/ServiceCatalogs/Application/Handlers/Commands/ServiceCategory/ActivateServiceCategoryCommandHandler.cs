using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class ActivateServiceCategoryCommandHandler : ICommandHandler<ActivateServiceCategoryCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ActivateServiceCategoryCommandHandler> _logger;

    public ActivateServiceCategoryCommandHandler(
        [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
        ILogger<ActivateServiceCategoryCommandHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(ActivateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        var uow = _uow;
        try
        {
            if (request.Id == Guid.Empty)
                return Result.Failure("Category ID cannot be empty.");

            var categoryId = ServiceCategoryId.From(request.Id);
            var category = await uow.GetRepository<Domain.Entities.ServiceCategory, ServiceCategoryId>().TryFindAsync(categoryId, cancellationToken);

            if (category is null)
                return Result.Failure($"Category with ID '{request.Id}' not found.");

            category.Activate();

            await uow.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro inesperado ao ativar a categoria de serviço.");
            return Result.Failure("Ocorreu um erro inesperado ao ativar a categoria de serviço.");
        }
    }
}



