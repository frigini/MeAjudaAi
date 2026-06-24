using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;

[ExcludeFromCodeCoverage]
public class ProviderBuilder : BaseBuilder<Provider>
{
    private Guid? _userId;
    private string? _name;
    private EProviderType? _type;
    private BusinessProfile? _businessProfile;
    private ProviderId? _providerId;
    private EVerificationStatus? _verificationStatus;
    private EProviderTier? _tier;
    private EProviderStatus? _status;
    private readonly List<Document> _documents = [];
    private readonly List<Qualification> _qualifications = [];
    private string? _city;
    private string? _state;

    public static ProviderBuilder Create() => new();

    public static ProviderBuilder CreateValid() => new ProviderBuilder()
        .WithName("John Provider")
        .WithType(EProviderType.Individual);

    public static ProviderBuilder CreateValidCompany() => new ProviderBuilder()
        .WithName("Acme Corp")
        .WithType(EProviderType.Company);

    public ProviderBuilder()
    {
        Faker = new Faker<Provider>()
            .CustomInstantiator(f =>
            {
                Provider provider;

                var businessProfile = _businessProfile ?? CreateDefaultBusinessProfile(f, _city, _state);

                provider = _providerId != null
                    ? new Provider(
                        _providerId,
                        _userId ?? f.Random.Guid(),
                        _name ?? f.Company.CompanyName(),
                        _type ?? f.PickRandom<EProviderType>(),
                        businessProfile
                    )
                    : Provider.Create(
                        _userId ?? f.Random.Guid(),
                        _name ?? f.Company.CompanyName(),
                        _type ?? f.PickRandom<EProviderType>(),
                        businessProfile
                    );

                foreach (var document in _documents)
                {
                    provider.AddDocument(document);
                }

                foreach (var qualification in _qualifications)
                {
                    provider.AddQualification(qualification);
                }

                if (_verificationStatus.HasValue)
                {
                    provider.UpdateVerificationStatus(_verificationStatus.Value);
                }

                if (_tier.HasValue && _tier.Value != EProviderTier.Standard)
                {
                    provider.PromoteTier(_tier.Value, "test-builder");
                }

                if (_status.HasValue && _status.Value != EProviderStatus.PendingBasicInfo)
                {
                    switch (_status.Value)
                    {
                        case EProviderStatus.PendingDocumentVerification:
                            provider.CompleteBasicInfo("test-builder");
                            break;
                        case EProviderStatus.Active:
                            provider.CompleteBasicInfo("test-builder");
                            provider.Activate("test-builder");
                            break;
                        case EProviderStatus.Suspended:
                            provider.CompleteBasicInfo("test-builder");
                            provider.Activate("test-builder");
                            provider.Suspend("Test suspension", "test-builder");
                            break;
                        case EProviderStatus.Rejected:
                            provider.Reject("Test rejection", "test-builder");
                            break;
                        case EProviderStatus.None:
                        case EProviderStatus.PendingBasicInfo:
                            break;
                        default:
                            break;
                    }
                }

                return provider;
            });
    }

    public ProviderBuilder WithTier(EProviderTier tier)
    {
        _tier = tier;
        return this;
    }

    public ProviderBuilder WithStatus(EProviderStatus status)
    {
        _status = status;
        return this;
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
        _documents.Add(new Document(Guid.NewGuid(), number, type));
        return this;
    }

    public ProviderBuilder WithDocument(string number, EDocumentType type, string fileName, string fileUrl)
    {
        _documents.Add(new Document(Guid.NewGuid(), number, type, fileName, fileUrl));
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

    public ProviderBuilder WithCity(string city)
    {
        _city = city;
        return this;
    }

    public ProviderBuilder WithState(string state)
    {
        _state = state;
        return this;
    }

    private bool _shouldDelete = false;
    private string? _deletedBy;

    public ProviderBuilder WithDeleted(bool isDeleted = true, string? deletedBy = null)
    {
        _shouldDelete = isDeleted;
        _deletedBy = deletedBy;
        return this;
    }

    public override Provider Build()
    {
        var provider = base.Build();

        if (_shouldDelete)
        {
            var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
            provider.Delete(fakeTimeProvider, _deletedBy);
        }

        return provider;
    }

    private static BusinessProfile CreateDefaultBusinessProfile(Faker faker, string? city = null, string? state = null)
    {
        var address = new Address(
            street: faker.Address.StreetAddress(),
            number: faker.Address.BuildingNumber(),
            neighborhood: "Centro",
            city: city ?? faker.Address.City(),
            state: state ?? faker.Address.StateAbbr(),
            zipCode: "12345-678",
            country: "Brasil");

        var contactInfo = new ContactInfo(
            email: faker.Internet.Email(),
            phoneNumber: "11999999999",
            website: faker.Internet.Url());

        return new BusinessProfile(
            legalName: faker.Company.CompanyName(),
            contactInfo: contactInfo,
            primaryAddress: address);
    }

    public static Provider CreateWithName(Guid userId, string? name, BusinessProfile businessProfile)
    {
        var providerId = new ProviderId(Guid.NewGuid());
        return new Provider(providerId, userId, name!, EProviderType.Individual, businessProfile);
    }

    public static Provider CreateWithBusinessProfile(Guid userId, string name, BusinessProfile? businessProfile)
    {
        var providerId = new ProviderId(Guid.NewGuid());
        return new Provider(providerId, userId, name, EProviderType.Individual, businessProfile!);
    }
}
