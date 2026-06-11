using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Communications.Application.Handlers.Commands;

public sealed class EmailTemplateCommandHandler([FromKeyedServices(ModuleKeys.Communications)] IUnitOfWork uow)
    : ICommandHandler<CreateEmailTemplateCommand, Result<Guid>>,
      ICommandHandler<UpdateEmailTemplateCommand, Result>,
      ICommandHandler<SetEmailTemplateStatusCommand, Result>
{
    public async Task<Result<Guid>> HandleAsync(CreateEmailTemplateCommand command, CancellationToken cancellationToken = default)
    {
        var template = EmailTemplate.Create(command.Key, command.Subject, command.HtmlBody, command.TextBody, command.Language, null, command.IsSystemTemplate);
        var repository = uow.GetRepository<EmailTemplate, Guid>();
        repository.Add(template);
        await uow.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(template.Id);
    }

    public async Task<Result> HandleAsync(UpdateEmailTemplateCommand command, CancellationToken cancellationToken = default)
    {
        var repository = uow.GetRepository<EmailTemplate, Guid>();
        var template = await repository.TryFindAsync(command.Id, cancellationToken);
        if (template == null) return Result.Failure(Error.NotFound("Template não encontrado."));

        // Desativa a versão atual
        template.Deactivate();

        // Cria nova versão com o conteúdo atualizado
        var newVersion = template.CreateNewVersion(command.Subject, command.HtmlBody, command.TextBody);
        repository.Add(newVersion);

        await uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> HandleAsync(SetEmailTemplateStatusCommand command, CancellationToken cancellationToken = default)
    {
        var repository = uow.GetRepository<EmailTemplate, Guid>();
        var template = await repository.TryFindAsync(command.Id, cancellationToken);
        if (template == null) return Result.Failure(Error.NotFound("Template não encontrado."));

        if (command.IsActive) template.Activate();
        else template.Deactivate();
        
        await uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
