using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Application.Queries;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Communications.Application.Handlers.Queries;

public sealed class GetAllEmailTemplatesHandler(IEmailTemplateQueries emailTemplateQueries) 
    : IQueryHandler<GetAllEmailTemplatesQuery, Result<IReadOnlyList<EmailTemplate>>>
{
    public async Task<Result<IReadOnlyList<EmailTemplate>>> HandleAsync(GetAllEmailTemplatesQuery query, CancellationToken cancellationToken = default)
    {
        var templates = await emailTemplateQueries.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<EmailTemplate>>.Success((IReadOnlyList<EmailTemplate>)templates);
    }
}
