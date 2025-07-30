namespace MeAjudai.Shared.Exceptions;

public class NotFoundException(string entityName, object entityId) : DomainException($"{entityName} with id {entityId} was not found")
{
    public string EntityName { get; } = entityName;
    public object EntityId { get; } = entityId;
}