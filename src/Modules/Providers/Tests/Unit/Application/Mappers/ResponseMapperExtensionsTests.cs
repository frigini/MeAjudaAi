using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using ProviderContactInfoDto = MeAjudaAi.Modules.Providers.Application.DTOs.ContactInfoDto;
using ProviderBusinessProfileDto = MeAjudaAi.Modules.Providers.Application.DTOs.BusinessProfileDto;
using ProviderAddressDto = MeAjudaAi.Modules.Providers.Application.DTOs.AddressDto;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
public class ResponseMapperExtensionsTests
{
    private static ProviderDto CreateTestProvider(
        string? email = "test@example.com",
        string? phone = "+5511999999999",
        string? deviceToken = "token123",
        EProviderType type = EProviderType.Company,
        EVerificationStatus verificationStatus = EVerificationStatus.Verified,
        bool isActive = true,
        bool isDeleted = false,
        List<DocumentDto>? documents = null)
    {
        var contactInfo = new ProviderContactInfoDto(email!, phone, "https://example.com");
        var address = new ProviderAddressDto("Main St", "123", "Suite 100", "Downtown", "São Paulo", "SP", "01310-000", "Brazil");
        var businessProfile = new ProviderBusinessProfileDto("Legal Name", "Fantasy", "Description", contactInfo, address);

        return new ProviderDto(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Name: "Test Provider",
            Slug: "test-provider",
            Type: type,
            BusinessProfile: businessProfile,
            Status: EProviderStatus.Active,
            VerificationStatus: verificationStatus,
            Tier: EProviderTier.Standard,
            Documents: documents ?? [],
            Qualifications: [],
            Services: [],
            CreatedAt: DateTime.UtcNow.AddDays(-30),
            UpdatedAt: DateTime.UtcNow,
            IsDeleted: isDeleted,
            DeletedAt: null,
            IsActive: isActive,
            DeviceToken: deviceToken);
    }

    [Fact]
    public void ToContract_WithCompleteProvider_ShouldMapAllProperties()
    {
        // Arrange
        var provider = CreateTestProvider();

        // Act
        var contract = provider.ToContract();

        // Assert
        contract.Should().NotBeNull();
        contract.Id.Should().Be(provider.Id);
        contract.Name.Should().Be("Test Provider");
        contract.Slug.Should().Be("test-provider");
        contract.Email.Should().Be("test@example.com");
        contract.Phone.Should().Be("+5511999999999");
        contract.DeviceToken.Should().Be("token123");
        contract.ProviderType.Should().Be("Company");
        contract.VerificationStatus.Should().Be("Verified");
        contract.IsActive.Should().BeTrue();
        contract.CreatedAt.Should().Be(provider.CreatedAt);
        contract.UpdatedAt.Should().Be(provider.UpdatedAt!.Value);
    }

    [Fact]
    public void ToContract_WithPrimaryDocument_ShouldMapDocumentNumber()
    {
        // Arrange
        var documents = new List<DocumentDto>
        {
            new("12345678900", EDocumentType.CPF, "doc.pdf", "https://storage/doc.pdf", IsPrimary: false),
            new("12345678000100", EDocumentType.CNPJ, "doc2.pdf", "https://storage/doc2.pdf", IsPrimary: true)
        };
        var provider = CreateTestProvider(documents: documents);

        // Act
        var contract = provider.ToContract();

        // Assert
        contract.Document.Should().Be("12345678000100");
    }

    [Fact]
    public void ToContract_WithNoPrimaryDocument_ShouldMapFirstDocument()
    {
        // Arrange
        var documents = new List<DocumentDto>
        {
            new("12345678900", EDocumentType.CPF, "doc.pdf", "https://storage/doc.pdf", IsPrimary: false)
        };
        var provider = CreateTestProvider(documents: documents);

        // Act
        var contract = provider.ToContract();

        // Assert
        contract.Document.Should().Be("12345678900");
    }

    [Fact]
    public void ToContract_WithNoDocuments_ShouldMapEmptyDocument()
    {
        // Arrange
        var provider = CreateTestProvider(documents: []);

        // Act
        var contract = provider.ToContract();

        // Assert
        contract.Document.Should().Be(string.Empty);
    }

    [Fact]
    public void ToContract_WithNullBusinessProfile_ShouldMapEmptyEmailAndPhone()
    {
        // Arrange
        var provider = new ProviderDto(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Name: "Test",
            Slug: "test",
            Type: EProviderType.Individual,
            BusinessProfile: null!,
            Status: EProviderStatus.Active,
            VerificationStatus: EVerificationStatus.Verified,
            Tier: EProviderTier.Standard,
            Documents: [],
            Qualifications: [],
            Services: [],
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null,
            IsDeleted: false,
            DeletedAt: null,
            IsActive: true);

        // Act
        var contract = provider.ToContract();

        // Assert
        contract.Email.Should().Be(string.Empty);
        contract.Phone.Should().BeNull();
    }

    [Fact]
    public void ToContract_WithNullUpdatedAt_ShouldUseCreatedAt()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-10);
        var provider = new ProviderDto(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Name: "Test",
            Slug: "test",
            Type: EProviderType.Individual,
            BusinessProfile: new ProviderBusinessProfileDto(
                "Legal",
                null,
                null,
                new ProviderContactInfoDto("test@example.com", null, null),
                null),
            Status: EProviderStatus.Active,
            VerificationStatus: EVerificationStatus.Verified,
            Tier: EProviderTier.Standard,
            Documents: [],
            Qualifications: [],
            Services: [],
            CreatedAt: createdAt,
            UpdatedAt: null,
            IsDeleted: false,
            DeletedAt: null,
            IsActive: true);

        // Act
        var contract = provider.ToContract();

        // Assert
        contract.UpdatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void ToBasicContract_WithCompleteProvider_ShouldMapAllProperties()
    {
        // Arrange
        var provider = CreateTestProvider();

        // Act
        var contract = provider.ToBasicContract();

        // Assert
        contract.Should().NotBeNull();
        contract.Id.Should().Be(provider.Id);
        contract.Name.Should().Be("Test Provider");
        contract.Slug.Should().Be("test-provider");
        contract.Email.Should().Be("test@example.com");
        contract.ProviderType.Should().Be("Company");
        contract.VerificationStatus.Should().Be("Verified");
        contract.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ToBasicContract_WithNullBusinessProfile_ShouldMapEmptyEmail()
    {
        // Arrange
        var provider = new ProviderDto(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Name: "Test",
            Slug: "test",
            Type: EProviderType.Individual,
            BusinessProfile: null!,
            Status: EProviderStatus.Active,
            VerificationStatus: EVerificationStatus.Verified,
            Tier: EProviderTier.Standard,
            Documents: [],
            Qualifications: [],
            Services: [],
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null,
            IsDeleted: false,
            DeletedAt: null,
            IsActive: true);

        // Act
        var contract = provider.ToBasicContract();

        // Assert
        contract.Email.Should().Be(string.Empty);
    }

    [Fact]
    public void ToBasicContract_Collection_ShouldMapAllItems()
    {
        // Arrange
        var providers = new List<ProviderDto>
        {
            CreateTestProvider(),
            CreateTestProvider(),
            CreateTestProvider()
        };

        // Act
        var contracts = providers.ToBasicContract();

        // Assert
        contracts.Should().HaveCount(3);
        contracts.Should().AllSatisfy(c =>
        {
            c.Name.Should().Be("Test Provider");
            c.Email.Should().Be("test@example.com");
        });
    }

    [Fact]
    public void ToBasicContract_EmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var providers = Enumerable.Empty<ProviderDto>();

        // Act
        var contracts = providers.ToBasicContract();

        // Assert
        contracts.Should().BeEmpty();
    }
}
