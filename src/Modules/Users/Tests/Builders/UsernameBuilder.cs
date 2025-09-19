using Bogus;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.Builders;

namespace MeAjudaAi.Modules.Users.Tests.Builders;

public class UsernameBuilder : BuilderBase<Username>
{
    public UsernameBuilder()
    {
        Faker = new Faker<Username>()
            .CustomInstantiator(f => new Username(f.Internet.UserName()));
    }

    public UsernameBuilder WithValue(string username)
    {
        Faker = new Faker<Username>()
            .CustomInstantiator(_ => new Username(username));
        return this;
    }

    public UsernameBuilder WithLength(int length)
    {
        if (length < 3 || length > 30)
            throw new ArgumentException("Username length must be between 3 and 30 characters");

        Faker = new Faker<Username>()
            .CustomInstantiator(f => new Username(f.Random.String2(length, "abcdefghijklmnopqrstuvwxyz0123456789")));
        return this;
    }

    public UsernameBuilder WithPrefix(string prefix)
    {
        Faker = new Faker<Username>()
            .CustomInstantiator(f => new Username($"{prefix}{f.Random.Number(100, 999)}"));
        return this;
    }

    public UsernameBuilder WithSuffix(string suffix)
    {
        Faker = new Faker<Username>()
            .CustomInstantiator(f => new Username($"{f.Random.String2(5, "abcdefghijklmnopqrstuvwxyz")}{suffix}"));
        return this;
    }

    public UsernameBuilder AsNumericOnly()
    {
        Faker = new Faker<Username>()
            .CustomInstantiator(f => new Username(f.Random.Number(100, 999999999).ToString()));
        return this;
    }

    public UsernameBuilder AsAlphaOnly()
    {
        Faker = new Faker<Username>()
            .CustomInstantiator(f => new Username(f.Random.String2(8, "abcdefghijklmnopqrstuvwxyz")));
        return this;
    }
}