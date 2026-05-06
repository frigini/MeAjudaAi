using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Mappings;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Database;
using IServiceCategoryQueries = MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.IServiceCategoryQueries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;

public sealed class GetAllServiceCategoriesQueryHandler : IQueryHandler<GetAllServiceCategoriesQuery, Result<IReadOnlyList<ServiceCategoryDto>>>
{
    private readonly IServiceCategoryQueries _queries;

    public GetAllServiceCategoriesQueryHandler(IServiceCategoryQueries queries)
    {
        _queries = queries;
    }

    public async Task<Result<IReadOnlyList<ServiceCategoryDto>>> HandleAsync(
        GetAllServiceCategoriesQuery request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _queries.GetAllAsync(request.ActiveOnly, cancellationToken);
            
            var dtos = categories.Select(c => c.ToDto()).ToList();
            
            return Result<IReadOnlyList<ServiceCategoryDto>>.Success(dtos);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<ServiceCategoryDto>>.Failure("Erro ao buscar categorias.");
        }
    }
}