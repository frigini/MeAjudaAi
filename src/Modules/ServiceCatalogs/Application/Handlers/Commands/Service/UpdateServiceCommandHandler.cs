using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class UpdateServiceCommandHandler : ICommandHandler<UpdateServiceCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly IServiceQueries _serviceQueries;
    private readonly ILogger<UpdateServiceCommandHandler> _logger;

    public UpdateServiceCommandHandler(
        [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
        IServiceQueries serviceQueries,
        ILogger<UpdateServiceCommandHandler> logger)
    {
        _uow = uow;
        _serviceQueries = serviceQueries;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(UpdateServiceCommand request, CancellationToken cancellationToken = default)
    {
        var uow = _uow;
        var serviceQueries = _serviceQueries;
        try
        {
            if (request.Id == Guid.Empty)
                return Result.Failure(ValidationMessages.Required.Id);

            var serviceId = ServiceId.From(request.Id);
            var service = await uow.GetRepository<Domain.Entities.Service, ServiceId>().TryFindAsync(serviceId, cancellationToken);

            if (service is null)
                return Result.Failure(Error.NotFound(ValidationMessages.NotFound.Service));

            var normalizedName = request.Name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                return Result.Failure(ValidationMessages.Required.ServiceName);

            // Verificar se já existe serviço com o mesmo nome na categoria (excluindo o serviço atual)
            if (await serviceQueries.ExistsWithNameAsync(normalizedName, serviceId, service.CategoryId, cancellationToken))
                return Result.Failure(string.Format(ValidationMessages.Catalogs.ServiceNameExists, normalizedName));

            service.Update(normalizedName, request.Description, request.DisplayOrder);

            await uow.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (CatalogDomainException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in UpdateServiceCommandHandler");
            return Result.Failure("An unexpected error occurred.");
        }
    }
}
