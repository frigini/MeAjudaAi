using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.Builders;
using MeAjudaAi.Shared.Time;
using Moq;

namespace MeAjudaAi.Modules.Providers.Tests.Builders;

public class ProviderBuilder : BuilderBase<Provider>
{
    private Guid? _userId;
    private string? _name;
    private EProviderType? _type;
    private BusinessProfile? _businessProfile;
    private ProviderId? _providerId;
    private EVerificationStatus? _verificationStatus;
    private readonly List<Document> _documents = new();
    private readonly List<Qualification> _qualifications = new();

    public static ProviderBuilder Create() => new();

    public ProviderBuilder()
    {
        // Configura o Faker com regras específicas para o domínio Provider
        Faker = new Faker<Provider>()
            .CustomInstantiator(f =>
            {
                Provider provider;

                // Se um ID específico foi fornecido, usa o construtor interno para testes
                if (_providerId != null)
                {
                    provider = new Provider(
                        _providerId,
                        _userId ?? f.Random.Guid(),
                        _name ?? f.Company.CompanyName(),
                        _type ?? f.PickRandom<EProviderType>(),
                        _businessProfile ?? CreateDefaultBusinessProfile(f)
                    );
                }
                else
                {
                    // Usa o construtor público que gera um novo ID
                    provider = new Provider(
                        _userId ?? f.Random.Guid(),
                        _name ?? f.Company.CompanyName(),
                        _type ?? f.PickRandom<EProviderType>(),
                        _businessProfile ?? CreateDefaultBusinessProfile(f)
                    );
                }

                // Adiciona documentos se especificados
                foreach (var document in _documents)
                {
                    provider.AddDocument(document);
                }

                // Adiciona qualificações se especificadas
                foreach (var qualification in _qualifications)
                {
                    provider.AddQualification(qualification);
                }

                // Define status de verificação se especificado
                if (_verificationStatus.HasValue)
                {
                    provider.UpdateVerificationStatus(_verificationStatus.Value);
                }

                return provider;
            });
    }

    public ProviderBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public ProviderBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProviderBuilder WithType(EProviderType type)
    {
        _type = type;
        return this;
    }

    public ProviderBuilder WithBusinessProfile(BusinessProfile businessProfile)
    {
        _businessProfile = businessProfile;
        return this;
    }

    public ProviderBuilder WithId(ProviderId id)
    {
        _providerId = id;
        return this;
    }

    public ProviderBuilder WithId(Guid id)
    {
        _providerId = new ProviderId(id);
        return this;
    }

    public ProviderBuilder WithVerificationStatus(EVerificationStatus status)
    {
        _verificationStatus = status;
        return this;
    }

    public ProviderBuilder WithDocument(string number, EDocumentType type)
    {
        _documents.Add(new Document(number, type));
        return this;
    }

    public ProviderBuilder WithQualification(string name, string description, string organization, DateTime issueDate, DateTime expirationDate, string documentNumber)
    {
        _qualifications.Add(new Qualification(name, description, organization, issueDate, expirationDate, documentNumber));
        return this;
    }

    public ProviderBuilder AsIndividual()
    {
        _type = EProviderType.Individual;
        return this;
    }

    public ProviderBuilder AsCompany()
    {
        _type = EProviderType.Company;
        return this;
    }

    private bool _shouldDelete = false;

    public ProviderBuilder WithDeleted(bool isDeleted = true)
    {
        _shouldDelete = isDeleted;
        return this;
    }

    public override Provider Build()
    {
        var provider = base.Build();
        
        if (_shouldDelete)
        {
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockDateTimeProvider.Setup(x => x.CurrentDate()).Returns(DateTime.UtcNow);
            provider.Delete(mockDateTimeProvider.Object);
        }
        
        return provider;
    }

    private static BusinessProfile CreateDefaultBusinessProfile(Faker faker)
    {
        var address = new Address(
            street: faker.Address.StreetAddress(),
            number: faker.Address.BuildingNumber(),
            neighborhood: "Centro",
            city: faker.Address.City(),
            state: faker.Address.StateAbbr(),
            zipCode: faker.Address.ZipCode(),
            country: "Brasil");

        var contactInfo = new ContactInfo(
            email: faker.Internet.Email(),
            phoneNumber: faker.Phone.PhoneNumber(),
            website: faker.Internet.Url());

        return new BusinessProfile(
            legalName: faker.Company.CompanyName(),
            contactInfo: contactInfo,
            primaryAddress: address);
    }
}
