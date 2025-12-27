using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders;

namespace MeAjudaAi.Modules.Users.Tests.Builders;

public class UsernameBuilder : BaseBuilder<Username>
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
        if (length < ValidationConstants.UserLimits.UsernameMinLength || length > ValidationConstants.UserLimits.UsernameMaxLength)
            throw new ArgumentException($"Username length must be between {ValidationConstants.UserLimits.UsernameMinLength} and {ValidationConstants.UserLimits.UsernameMaxLength} characters");

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
