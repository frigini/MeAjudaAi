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

                // Se um ID específico foi definido, usa helper interno
                if (_id.HasValue)
                {
                    user.SetIdForTesting(new UserId(_id.Value));
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
    /// Marca o usuário como excluído usando o horário UTC atual.
    /// Para testes que exigem timestamps de exclusão específicos, use AsDeleted(DateTime).
    /// </summary>
    public UserBuilder AsDeleted()
    {
        var dateTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        WithCustomAction(user => user.MarkAsDeleted(dateTimeProvider));
        return this;
    }

    /// <summary>
    /// Marca o usuário como excluído com um timestamp de exclusão específico.
    /// Útil para testes que precisam validar valores exatos de DeletedAt.
    /// </summary>
    public UserBuilder AsDeleted(DateTime deletedAt)
    {
        var dateTimeProvider = new FakeTimeProvider(new DateTimeOffset(deletedAt, TimeSpan.Zero));
        WithCustomAction(user => user.MarkAsDeleted(dateTimeProvider));
        return this;
    }

    public UserBuilder WithCreatedAt(DateTime createdAt)
    {
        WithCustomAction(user => user.SetCreatedAtForTesting(createdAt));
        return this;
    }

    public UserBuilder WithUpdatedAt(DateTime? updatedAt)
    {
        WithCustomAction(user => user.SetUpdatedAtForTesting(updatedAt));
        return this;
    }
}
