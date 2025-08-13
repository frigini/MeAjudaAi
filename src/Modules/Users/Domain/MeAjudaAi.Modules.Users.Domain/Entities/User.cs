using MeAjudaAi.Modules.Users.Domain.Enums;
using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.ValuleObjects;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Domain.Entities;

public class User : AggregateRoot<UserId>
{
    private int _version = 0;

    public Email Email { get; private set; }
    public UserProfile Profile { get; private set; }
    public EUserStatus Status { get; private set; }
    public string KeycloakId { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public List<string> Roles { get; private set; } = [];

    private User() { } // EF Constructor

    public User(UserId id, Email email, UserProfile profile, string keycloakId)
    {
        Id = id;
        Email = email;
        Profile = profile;
        KeycloakId = keycloakId;
        Status = EUserStatus.PendingVerification;
        _version++;

        AddDomainEvent(new UserRegisteredDomainEvent(
            Id.Value,
            _version,
            Email.Value,
            Profile.FirstName,
            Profile.LastName
        ));
    }

    public void UpdateProfile(UserProfile newProfile)
    {
        Profile = newProfile;
        _version++;
        MarkAsUpdated();

        AddDomainEvent(new UserProfileUpdatedDomainEvent(
            Id.Value,
            _version,
            Profile.FirstName,
            Profile.LastName
        ));
    }

    public void AssignRole(string role)
    {
        if (!Roles.Contains(role))
        {
            var previousRoles = string.Join(",", Roles);
            Roles.Add(role);
            _version++;
            MarkAsUpdated();

            AddDomainEvent(new UserRoleChangedDomainEvent(
                Id.Value,
                _version,
                previousRoles,
                role,
                "System"
            ));
        }
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Activate()
    {
        Status = EUserStatus.Active;
        _version++;
        MarkAsUpdated();
    }
}