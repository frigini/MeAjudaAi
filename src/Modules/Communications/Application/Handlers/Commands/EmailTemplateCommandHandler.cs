using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;

namespace MeAjudaAi.Modules.Communications.Application.Handlers.Commands;

public sealed class EmailTemplateCommandHandler(
    IRepository<EmailTemplate, Guid> templateRepository,
    IUnitOfWork uow)
    : ICommandHandler<CreateEmailTemplateCommand, Result<Guid>>,
      ICommandHandler<UpdateEmailTemplateCommand, Result>,
      ICommandHandler<SetEmailTemplateStatusCommand, Result>
{
    public async Task<Result<Guid>> HandleAsync(CreateEmailTemplateCommand command, CancellationToken cancellationToken = default)
    {
        var template = EmailTemplate.Create(command.Key, command.Subject, command.HtmlBody, command.TextBody, command.Language, null, command.IsSystemTemplate);
        templateRepository.Add(template);
        await uow.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(template.Id);
    }

    public async Task<Result> HandleAsync(UpdateEmailTemplateCommand command, CancellationToken cancellationToken = default)
    {
        var template = await templateRepository.TryFindAsync(command.Id, cancellationToken);
        if (template == null) return Result.Failure(Error.NotFound("Template não encontrado."));

        template.UpdateContent(command.Subject, command.HtmlBody, command.TextBody);
        await uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> HandleAsync(SetEmailTemplateStatusCommand command, CancellationToken cancellationToken = default)
    {
        var template = await templateRepository.TryFindAsync(command.Id, cancellationToken);
        if (template == null) return Result.Failure(Error.NotFound("Template não encontrado."));

        if (command.IsActive) template.Activate();
        else template.Deactivate();
        
        await uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
