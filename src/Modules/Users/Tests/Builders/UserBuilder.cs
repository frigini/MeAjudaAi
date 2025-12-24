using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders;
using Microsoft.Extensions.Time.Testing;


namespace MeAjudaAi.Modules.Users.Tests.Builders;

public class UserBuilder : BaseBuilder<User>
{
    private Username? _username;
    private Email? _email;
    private string? _firstName;
    private string? _lastName;
    private string? _keycloakId;
    private Guid? _id;

    public UserBuilder()
    {
        // Configura o Faker com regras específicas para o domínio User
        Faker = new Faker<User>()
            .CustomInstantiator(f =>
            {
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
                    idField?.SetValue(user, new UserId(_id.Value));
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

    /// <summary>
    /// Marks the user as deleted using current UTC time.
    /// For tests requiring specific deletion timestamps, use AsDeleted(DateTime).
    /// </summary>
    public UserBuilder AsDeleted()
    {
        var dateTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        WithCustomAction(user => user.MarkAsDeleted(dateTimeProvider));
        return this;
    }

    /// <summary>
    /// Marks the user as deleted with a specific deletion timestamp.
    /// Useful for tests that need to assert exact DeletedAt values.
    /// </summary>
    public UserBuilder AsDeleted(DateTime deletedAt)
    {
        var dateTimeProvider = new MockDateTimeProvider(deletedAt);
        WithCustomAction(user => user.MarkAsDeleted(dateTimeProvider));
        return this;
    }

    public UserBuilder WithCreatedAt(DateTime createdAt)
    {
        WithCustomAction(user =>
        {
            var createdAtProperty = typeof(User).GetProperty("CreatedAt", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (createdAtProperty != null && createdAtProperty.CanWrite)
            {
                createdAtProperty.SetValue(user, createdAt);
            }
            else
            {
                // Se a propriedade não é writable, tenta usar reflection no campo backing
                var createdAtField = typeof(User).BaseType?.GetField("<CreatedAt>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                createdAtField?.SetValue(user, createdAt);
            }
        });
        return this;
    }

    public UserBuilder WithUpdatedAt(DateTime? updatedAt)
    {
        WithCustomAction(user =>
        {
            var updatedAtProperty = typeof(User).GetProperty("UpdatedAt", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (updatedAtProperty != null && updatedAtProperty.CanWrite)
            {
                updatedAtProperty.SetValue(user, updatedAt);
            }
            else
            {
                // Se a propriedade não é writable, tenta usar reflection no campo backing
                var updatedAtField = typeof(User).BaseType?.GetField("<UpdatedAt>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                updatedAtField?.SetValue(user, updatedAt);
            }
        });
        return this;
    }
}
