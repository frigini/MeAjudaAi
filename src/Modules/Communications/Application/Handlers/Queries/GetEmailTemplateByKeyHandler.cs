using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Application.Queries;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Communications.Application.Handlers.Queries;

public sealed class GetEmailTemplateByKeyHandler(IEmailTemplateQueries emailTemplateQueries) 
    : IQueryHandler<GetEmailTemplateByKeyQuery, Result<EmailTemplate?>>
{
    public async Task<Result<EmailTemplate?>> HandleAsync(GetEmailTemplateByKeyQuery query, CancellationToken cancellationToken = default)
    {
        var template = await emailTemplateQueries.GetActiveByKeyAsync(query.TemplateKey, query.Language, cancellationToken);
        return Result<EmailTemplate?>.Success(template);
    }
}
