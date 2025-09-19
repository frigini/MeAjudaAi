using Bogus;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.Builders;

namespace MeAjudaAi.Modules.Users.Tests.Builders;

public class EmailBuilder : BuilderBase<Email>
{
    public EmailBuilder()
    {
        Faker = new Faker<Email>()
            .CustomInstantiator(f => new Email(f.Internet.Email()));
    }

    public EmailBuilder WithValue(string email)
    {
        Faker = new Faker<Email>()
            .CustomInstantiator(_ => new Email(email));
        return this;
    }

    public EmailBuilder WithDomain(string domain)
    {
        Faker = new Faker<Email>()
            .CustomInstantiator(f => new Email($"{f.Internet.UserName()}@{domain}"));
        return this;
    }

    public EmailBuilder AsGmail()
    {
        return WithDomain("gmail.com");
    }

    public EmailBuilder AsOutlook()
    {
        return WithDomain("outlook.com");
    }

    public EmailBuilder AsCompanyEmail(string company)
    {
        return WithDomain($"{company}.com");
    }
}