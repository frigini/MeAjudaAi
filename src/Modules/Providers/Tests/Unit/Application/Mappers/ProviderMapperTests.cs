using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
public class ProviderMapperTests
{
    [Fact]
    public void ToDto_WithCompleteProvider_ShouldMapAllProperties()
    {
        // Arrange
        var provider = ProviderBuilder.Create()
            .WithName("Test Provider")
            .WithType(EProviderType.Individual)
            .WithBusinessProfile(new BusinessProfile(
                "Legal Name Ltd",
                new ContactInfo("test@example.com", "+5511999999999", "https://example.com"),
                new Address("Main St", "123", "Downtown", "São Paulo", "SP", "01310-000", "Brazil", "Suite 100"),
                "Fantasy Name",
                "Business description"))
            .Build();

        // Act
        var dto = provider.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(provider.Id.Value);
        dto.UserId.Should().Be(provider.UserId);
        dto.Name.Should().Be("Test Provider");
        dto.Type.Should().Be(EProviderType.Individual);
        dto.Status.Should().Be(provider.Status);
        dto.VerificationStatus.Should().Be(provider.VerificationStatus);
        dto.CreatedAt.Should().Be(provider.CreatedAt);
        dto.UpdatedAt.Should().Be(provider.UpdatedAt);
        dto.IsDeleted.Should().Be(provider.IsDeleted);
        dto.DeletedAt.Should().Be(provider.DeletedAt);
        dto.SuspensionReason.Should().Be(provider.SuspensionReason);
        dto.RejectionReason.Should().Be(provider.RejectionReason);
    }

    [Fact]
    public void ToDto_WithBusinessProfile_ShouldMapNestedProperties()
    {
        // Arrange
        var businessProfile = new BusinessProfile(
            "Legal Name Ltd",
            new ContactInfo("test@example.com", "+5511999999999", "https://example.com"),
            new Address("Main St", "123", "Downtown", "São Paulo", "SP", "01310-000", "Brazil", "Suite 100"),
            "Fantasy Name",
            "Business description");

        // Act
        var dto = businessProfile.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.LegalName.Should().Be("Legal Name Ltd");
        dto.FantasyName.Should().Be("Fantasy Name");
        dto.Description.Should().Be("Business description");
        dto.ContactInfo.Should().NotBeNull();
        dto.PrimaryAddress.Should().NotBeNull();
    }

    [Fact]
    public void ToDto_WithContactInfo_ShouldMapAllFields()
    {
        // Arrange
        var contactInfo = new ContactInfo("test@example.com", "+5511999999999", "https://example.com");

        // Act
        var dto = contactInfo.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Email.Should().Be("test@example.com");
        dto.PhoneNumber.Should().Be("+5511999999999");
        dto.Website.Should().Be("https://example.com");
    }

    [Fact]
    public void ToDto_WithAddress_ShouldMapAllFields()
    {
        // Arrange
        var address = new Address("Main St", "123", "Downtown", "São Paulo", "SP", "01310-000", "Brazil", "Suite 100");

        // Act
        var dto = address.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Street.Should().Be("Main St");
        dto.Number.Should().Be("123");
        dto.Complement.Should().Be("Suite 100");
        dto.Neighborhood.Should().Be("Downtown");
        dto.City.Should().Be("São Paulo");
        dto.State.Should().Be("SP");
        dto.ZipCode.Should().Be("01310-000");
        dto.Country.Should().Be("Brazil");
    }

    [Fact]
    public void ToDto_WithDocument_ShouldMapAllFields()
    {
        // Arrange
        var document = new Document("12345678900", EDocumentType.CPF, isPrimary: true);

        // Act
        var dto = document.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Number.Should().Be("12345678900");
        dto.DocumentType.Should().Be(EDocumentType.CPF);
        dto.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void ToDto_WithNonPrimaryDocument_ShouldMapIsPrimaryAsFalse()
    {
        // Arrange
        var document = new Document("12345678900", EDocumentType.CPF, isPrimary: false);

        // Act
        var dto = document.ToDto();

        // Assert
        dto.IsPrimary.Should().BeFalse();
    }

    [Fact]
    public void ToDto_WithQualification_ShouldMapAllFields()
    {
        // Arrange
        var issueDate = new DateTime(2020, 1, 1);
        var expirationDate = new DateTime(2025, 1, 1);
        var qualification = new Qualification(
            "Professional Certificate",
            "Advanced training in healthcare",
            "Health Ministry",
            issueDate,
            expirationDate);

        // Act
        var dto = qualification.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Name.Should().Be("Professional Certificate");
        dto.Description.Should().Be("Advanced training in healthcare");
        dto.IssuingOrganization.Should().Be("Health Ministry");
        dto.IssueDate.Should().Be(issueDate);
        dto.ExpirationDate.Should().Be(expirationDate);
    }

    [Fact]
    public void ToDto_WithProviderCollection_ShouldMapAllProviders()
    {
        // Arrange
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithName("Provider 1").Build(),
            ProviderBuilder.Create().WithName("Provider 2").Build(),
            ProviderBuilder.Create().WithName("Provider 3").Build()
        };

        // Act
        var dtos = providers.ToDto();

        // Assert
        dtos.Should().HaveCount(3);
        dtos[0].Name.Should().Be("Provider 1");
        dtos[1].Name.Should().Be("Provider 2");
        dtos[2].Name.Should().Be("Provider 3");
    }

    [Fact]
    public void ToDto_WithEmptyProviderCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var providers = new List<Provider>();

        // Act
        var dtos = providers.ToDto();

        // Assert
        dtos.Should().BeEmpty();
    }

    [Fact]
    public void ToDomain_WithBusinessProfileDto_ShouldMapAllProperties()
    {
        // Arrange
        var contactInfoDto = new ContactInfoDto("test@example.com", "+5511999999999", "https://example.com");
        var addressDto = new AddressDto("Main St", "123", "Suite 100", "Downtown", "São Paulo", "SP", "01310-000", "Brazil");
        var dto = new BusinessProfileDto("Legal Name Ltd", "Fantasy Name", "Business description", contactInfoDto, addressDto);

        // Act
        var businessProfile = dto.ToDomain();

        // Assert
        businessProfile.Should().NotBeNull();
        businessProfile.LegalName.Should().Be("Legal Name Ltd");
        businessProfile.FantasyName.Should().Be("Fantasy Name");
        businessProfile.Description.Should().Be("Business description");
        businessProfile.ContactInfo.Email.Should().Be("test@example.com");
        businessProfile.PrimaryAddress.Street.Should().Be("Main St");
    }

    [Fact]
    public void ToDomain_WithDocumentDto_ShouldMapAllProperties()
    {
        // Arrange
        var dto = new DocumentDto("12345678900", EDocumentType.CPF, true);

        // Act
        var document = dto.ToDomain();

        // Assert
        document.Should().NotBeNull();
        document.Number.Should().Be("12345678900");
        document.DocumentType.Should().Be(EDocumentType.CPF);
        document.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void ToDomain_WithQualificationDto_ShouldMapAllProperties()
    {
        // Arrange
        var issueDate = new DateTime(2020, 1, 1);
        var expirationDate = new DateTime(2025, 1, 1);
        var dto = new QualificationDto(
            "Professional Certificate",
            "Advanced training in healthcare",
            "Health Ministry",
            issueDate,
            expirationDate,
            "CERT-12345");

        // Act
        var qualification = dto.ToDomain();

        // Assert
        qualification.Should().NotBeNull();
        qualification.Name.Should().Be("Professional Certificate");
        qualification.Description.Should().Be("Advanced training in healthcare");
        qualification.IssuingOrganization.Should().Be("Health Ministry");
        qualification.IssueDate.Should().Be(issueDate);
        qualification.ExpirationDate.Should().Be(expirationDate);
    }

    [Fact]
    public void ToDomain_WithNullContactInfo_ShouldThrowArgumentNullException()
    {
        // Arrange
        var addressDto = new AddressDto("Main St", "123", "Suite 100", "Downtown", "São Paulo", "SP", "01310-000", "Brazil");
        var dto = new BusinessProfileDto("Legal Name Ltd", "Fantasy Name", "Business description", null!, addressDto);

        // Act
        var act = () => dto.ToDomain();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToDomain_WithNullPrimaryAddress_ShouldThrowArgumentNullException()
    {
        // Arrange
        var contactInfoDto = new ContactInfoDto("test@example.com", "+5511999999999", "https://example.com");
        var dto = new BusinessProfileDto("Legal Name Ltd", "Fantasy Name", "Business description", contactInfoDto, null!);

        // Act
        var act = () => dto.ToDomain();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToDto_WithProviderWithDocuments_ShouldMapDocuments()
    {
        // Arrange
        var provider = ProviderBuilder.Create()
            .WithDocument("12345678900", EDocumentType.CPF)
            .WithDocument("12345678000100", EDocumentType.CNPJ)
            .Build();

        // Act
        var dto = provider.ToDto();

        // Assert
        dto.Documents.Should().HaveCount(2);
        dto.Documents.Should().Contain(d => d.DocumentType == EDocumentType.CPF);
        dto.Documents.Should().Contain(d => d.DocumentType == EDocumentType.CNPJ);
    }

    [Fact]
    public void ToDto_WithProviderWithQualifications_ShouldMapQualifications()
    {
        // Arrange
        var provider = ProviderBuilder.Create()
            .WithQualification("Certificate 1", "Description 1", "Organization 1", DateTime.UtcNow.AddYears(-2), DateTime.UtcNow.AddYears(3), "DOC-001")
            .WithQualification("Certificate 2", "Description 2", "Organization 2", DateTime.UtcNow.AddYears(-1), DateTime.UtcNow.AddYears(4), "DOC-002")
            .Build();

        // Act
        var dto = provider.ToDto();

        // Assert
        dto.Qualifications.Should().HaveCount(2);
        dto.Qualifications.Should().Contain(q => q.Name == "Certificate 1");
        dto.Qualifications.Should().Contain(q => q.Name == "Certificate 2");
    }

    [Fact]
    public void ToDto_WithMinimalBusinessProfile_ShouldHandleOptionalFields()
    {
        // Arrange
        var businessProfile = new BusinessProfile(
            "Legal Name Ltd",
            new ContactInfo("test@example.com", null, null),
            new Address("Main St", "123", "Downtown", "São Paulo", "SP", "01310-000", "Brazil", null),
            null,
            null);

        // Act
        var dto = businessProfile.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.LegalName.Should().Be("Legal Name Ltd");
        dto.FantasyName.Should().BeNull();
        dto.Description.Should().BeNull();
        dto.ContactInfo.PhoneNumber.Should().BeNull();
        dto.ContactInfo.Website.Should().BeNull();
        dto.PrimaryAddress.Complement.Should().BeNull();
    }
}
