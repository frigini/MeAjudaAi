using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Domain.Entities;

public sealed class User : AggregateRoot<UserId>
{
    public Username Username { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string KeycloakId { get; private set; } = string.Empty; // External ID

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private User() { } // EF Constructor

    public User(Username username, Email email, string firstName, string lastName, string keycloakId)
        : base(UserId.New())
    {
        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        KeycloakId = keycloakId;

        AddDomainEvent(new UserRegisteredDomainEvent(Id.Value, 1, email.Value, username.Value, firstName, lastName));
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        if (FirstName == firstName && LastName == lastName)
            return;

        FirstName = firstName;
        LastName = lastName;
        MarkAsUpdated();

        AddDomainEvent(new UserProfileUpdatedDomainEvent(Id.Value, 1, firstName, lastName));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        MarkAsUpdated();

        AddDomainEvent(new UserDeletedDomainEvent(Id.Value, 1));
    }

    public string GetFullName() => $"{FirstName} {LastName}".Trim();
}