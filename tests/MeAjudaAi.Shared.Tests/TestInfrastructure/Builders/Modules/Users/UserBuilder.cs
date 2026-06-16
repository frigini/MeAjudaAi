using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Users;

[ExcludeFromCodeCoverage]
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
        Faker = new Faker<User>()
            .CustomInstantiator(f =>
            {
                var userResult = User.Create(
                    _username ?? new Username(f.Internet.UserName()),
                    _email ?? new Email(f.Internet.Email()),
                    _firstName ?? f.Name.FirstName(),
                    _lastName ?? f.Name.LastName(),
                    _keycloakId ?? Guid.NewGuid().ToString()
                );

                if (userResult.IsFailure)
                {
                    throw new InvalidOperationException(userResult.Error?.Message ?? "User creation failed");
                }

                var user = userResult.Value ?? throw new InvalidOperationException("User creation returned null");

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

    public UserBuilder AsDeleted()
    {
        var dateTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        WithCustomAction(user => user.MarkAsDeleted(dateTimeProvider));
        return this;
    }

    public UserBuilder AsDeleted(DateTime deletedAt)
    {
        var dateTimeProvider = new FakeTimeProvider(new DateTimeOffset(deletedAt, TimeSpan.Zero));
        WithCustomAction(user => user.MarkAsDeleted(dateTimeProvider));
        return this;
    }
}
