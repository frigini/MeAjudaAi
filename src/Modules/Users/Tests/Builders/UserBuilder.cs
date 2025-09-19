using Bogus;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.Builders;

namespace MeAjudaAi.Modules.Users.Tests.Builders;

public class UserBuilder : BuilderBase<User>
{
    private Username? _username;
    private Email? _email;
    private string? _firstName;
    private string? _lastName;
    private string? _keycloakId;
    private Guid? _id;

    public UserBuilder()
    {
        // Configure Faker with specific rules for User domain
        Faker = new Faker<User>()
            .CustomInstantiator(f => {
                var user = new User(
                    _username ?? new Username(f.Internet.UserName()),
                    _email ?? new Email(f.Internet.Email()),
                    _firstName ?? f.Name.FirstName(),
                    _lastName ?? f.Name.LastName(),
                    _keycloakId ?? f.Random.Guid().ToString()
                );

                // Se um ID específico foi definido, define através de reflexão
                if (_id.HasValue)
                {
                    var idField = typeof(User).GetField("_id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (idField != null)
                    {
                        idField.SetValue(user, new UserId(_id.Value));
                    }
                }

                return user;
            });
    }

    public UserBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        _username = new Username(username);
        return this;
    }

    public UserBuilder WithUsername(Username username)
    {
        _username = username;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = new Email(email);
        return this;
    }

    public UserBuilder WithEmail(Email email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public UserBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public UserBuilder WithFullName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }

    public UserBuilder WithKeycloakId(string keycloakId)
    {
        _keycloakId = keycloakId;
        return this;
    }

    public UserBuilder AsDeleted()
    {
        WithCustomAction(user => user.MarkAsDeleted());
        return this;
    }
}