using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Persistence.Configurations;

/// <summary>
/// Testes unitários para ProviderConfiguration (EF Core entity configuration).
/// Valida mapeamento de tabela, propriedades, conversões, owned entities, índices e relacionamentos.
/// </summary>
public sealed class ProviderConfigurationTests : IDisposable
{
    private readonly DbContext _context;
    private readonly IEntityType _entityType;

    public ProviderConfigurationTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _entityType = _context.Model.FindEntityType(typeof(Provider))!;
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Table Configuration Tests

    [Fact]
    public void Configure_ShouldMapToProvidersTable()
    {
        // Assert
        _entityType.GetTableName().Should().Be("providers");
    }

    [Fact]
    public void Configure_ShouldHaveIdAsPrimaryKey()
    {
        // Assert
        var primaryKey = _entityType.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties.First().Name.Should().Be("Id");
    }

    #endregion

    #region Property Configuration Tests

    [Fact]
    public void Configure_IdProperty_ShouldHaveCorrectColumnName()
    {
        // Arrange
        var property = _entityType.FindProperty("Id");

        // Assert
        property.Should().NotBeNull();
        property!.GetColumnName().Should().Be("id");
    }

    [Fact]
    public void Configure_IdProperty_ShouldHaveValueConverter()
    {
        // Arrange
        var property = _entityType.FindProperty("Id");

        // Assert
        property.Should().NotBeNull();
        property!.GetValueConverter().Should().NotBeNull();
    }

    [Fact]
    public void Configure_UserIdProperty_ShouldHaveCorrectColumnName()
    {
        // Arrange
        var property = _entityType.FindProperty("UserId");

        // Assert
        property.Should().NotBeNull();
        property!.GetColumnName().Should().Be("user_id");
    }

    [Fact]
    public void Configure_NameProperty_ShouldBeRequiredWithMaxLength100()
    {
        // Arrange
        var property = _entityType.FindProperty("Name");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(100);
        property.GetColumnName().Should().Be("name");
    }

    [Fact]
    public void Configure_TypeProperty_ShouldHaveEnumConversion()
    {
        // Arrange
        var property = _entityType.FindProperty("Type");

        // Assert
        property.Should().NotBeNull();
        property!.GetValueConverter().Should().NotBeNull();
        property.GetMaxLength().Should().Be(20);
        property.IsNullable.Should().BeFalse();
        property.GetColumnName().Should().Be("type");
    }

    [Fact]
    public void Configure_StatusProperty_ShouldHaveEnumConversion()
    {
        // Arrange
        var property = _entityType.FindProperty("Status");

        // Assert
        property.Should().NotBeNull();
        property!.GetValueConverter().Should().NotBeNull();
        property.GetMaxLength().Should().Be(30);
        property.IsNullable.Should().BeFalse();
        property.GetColumnName().Should().Be("status");
    }

    [Fact]
    public void Configure_VerificationStatusProperty_ShouldHaveEnumConversion()
    {
        // Arrange
        var property = _entityType.FindProperty("VerificationStatus");

        // Assert
        property.Should().NotBeNull();
        property!.GetValueConverter().Should().NotBeNull();
        property.GetMaxLength().Should().Be(20);
        property.IsNullable.Should().BeFalse();
        property.GetColumnName().Should().Be("verification_status");
    }

    [Fact]
    public void Configure_IsDeletedProperty_ShouldBeRequired()
    {
        // Arrange
        var property = _entityType.FindProperty("IsDeleted");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetColumnName().Should().Be("is_deleted");
    }

    [Fact]
    public void Configure_DeletedAtProperty_ShouldBeNullable()
    {
        // Arrange
        var property = _entityType.FindProperty("DeletedAt");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeTrue();
        property.GetColumnName().Should().Be("deleted_at");
    }

    [Fact]
    public void Configure_SuspensionReasonProperty_ShouldHaveMaxLength1000()
    {
        // Arrange
        var property = _entityType.FindProperty("SuspensionReason");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(1000);
        property.GetColumnName().Should().Be("suspension_reason");
    }

    [Fact]
    public void Configure_RejectionReasonProperty_ShouldHaveMaxLength1000()
    {
        // Arrange
        var property = _entityType.FindProperty("RejectionReason");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(1000);
        property.GetColumnName().Should().Be("rejection_reason");
    }

    [Fact]
    public void Configure_CreatedAtProperty_ShouldBeRequired()
    {
        // Arrange
        var property = _entityType.FindProperty("CreatedAt");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetColumnName().Should().Be("created_at");
    }

    [Fact]
    public void Configure_UpdatedAtProperty_ShouldBeNullable()
    {
        // Arrange
        var property = _entityType.FindProperty("UpdatedAt");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeTrue();
        property.GetColumnName().Should().Be("updated_at");
    }

    #endregion

    #region BusinessProfile Owned Entity Tests

    [Fact]
    public void Configure_BusinessProfile_ShouldBeOwnedEntity()
    {
        // Arrange
        var navigation = _entityType.FindNavigation("BusinessProfile");

        // Assert
        navigation.Should().NotBeNull();
        navigation!.ForeignKey.IsOwnership.Should().BeTrue();
    }

    [Fact]
    public void Configure_BusinessProfile_LegalNameProperty_ShouldBeRequiredWithMaxLength200()
    {
        // Arrange
        var ownedType = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var property = ownedType.FindProperty("LegalName");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(200);
        property.GetColumnName().Should().Be("legal_name");
    }

    [Fact]
    public void Configure_BusinessProfile_FantasyNameProperty_ShouldHaveMaxLength200()
    {
        // Arrange
        var ownedType = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var property = ownedType.FindProperty("FantasyName");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(200);
        property.GetColumnName().Should().Be("fantasy_name");
    }

    [Fact]
    public void Configure_BusinessProfile_DescriptionProperty_ShouldHaveMaxLength1000()
    {
        // Arrange
        var ownedType = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var property = ownedType.FindProperty("Description");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(1000);
        property.GetColumnName().Should().Be("description");
    }

    #endregion

    #region ContactInfo Owned Entity Tests

    [Fact]
    public void Configure_ContactInfo_ShouldBeOwnedEntity()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var navigation = businessProfile.FindNavigation("ContactInfo");

        // Assert
        navigation.Should().NotBeNull();
        navigation!.ForeignKey.IsOwnership.Should().BeTrue();
    }

    [Fact]
    public void Configure_ContactInfo_EmailProperty_ShouldBeRequiredWithMaxLength255()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var contactInfo = businessProfile.FindNavigation("ContactInfo")!.TargetEntityType;
        var property = contactInfo.FindProperty("Email");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(255);
        property.GetColumnName().Should().Be("email");
    }

    [Fact]
    public void Configure_ContactInfo_PhoneNumberProperty_ShouldHaveMaxLength20()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var contactInfo = businessProfile.FindNavigation("ContactInfo")!.TargetEntityType;
        var property = contactInfo.FindProperty("PhoneNumber");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(20);
        property.GetColumnName().Should().Be("phone_number");
    }

    [Fact]
    public void Configure_ContactInfo_WebsiteProperty_ShouldHaveMaxLength255()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var contactInfo = businessProfile.FindNavigation("ContactInfo")!.TargetEntityType;
        var property = contactInfo.FindProperty("Website");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(255);
        property.GetColumnName().Should().Be("website");
    }

    #endregion

    #region PrimaryAddress Owned Entity Tests

    [Fact]
    public void Configure_PrimaryAddress_ShouldBeOwnedEntity()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var navigation = businessProfile.FindNavigation("PrimaryAddress");

        // Assert
        navigation.Should().NotBeNull();
        navigation!.ForeignKey.IsOwnership.Should().BeTrue();
    }

    [Fact]
    public void Configure_PrimaryAddress_StreetProperty_ShouldBeRequiredWithMaxLength200()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var address = businessProfile.FindNavigation("PrimaryAddress")!.TargetEntityType;
        var property = address.FindProperty("Street");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(200);
        property.GetColumnName().Should().Be("street");
    }

    [Fact]
    public void Configure_PrimaryAddress_NumberProperty_ShouldBeRequiredWithMaxLength20()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var address = businessProfile.FindNavigation("PrimaryAddress")!.TargetEntityType;
        var property = address.FindProperty("Number");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(20);
        property.GetColumnName().Should().Be("number");
    }

    [Fact]
    public void Configure_PrimaryAddress_ComplementProperty_ShouldHaveMaxLength100()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var address = businessProfile.FindNavigation("PrimaryAddress")!.TargetEntityType;
        var property = address.FindProperty("Complement");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(100);
        property.GetColumnName().Should().Be("complement");
    }

    [Fact]
    public void Configure_PrimaryAddress_NeighborhoodProperty_ShouldBeRequiredWithMaxLength100()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var address = businessProfile.FindNavigation("PrimaryAddress")!.TargetEntityType;
        var property = address.FindProperty("Neighborhood");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(100);
        property.GetColumnName().Should().Be("neighborhood");
    }

    [Fact]
    public void Configure_PrimaryAddress_CityProperty_ShouldBeRequiredWithMaxLength100()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var address = businessProfile.FindNavigation("PrimaryAddress")!.TargetEntityType;
        var property = address.FindProperty("City");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(100);
        property.GetColumnName().Should().Be("city");
    }

    [Fact]
    public void Configure_PrimaryAddress_StateProperty_ShouldBeRequiredWithMaxLength50()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var address = businessProfile.FindNavigation("PrimaryAddress")!.TargetEntityType;
        var property = address.FindProperty("State");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(50);
        property.GetColumnName().Should().Be("state");
    }

    [Fact]
    public void Configure_PrimaryAddress_ZipCodeProperty_ShouldBeRequiredWithMaxLength20()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var address = businessProfile.FindNavigation("PrimaryAddress")!.TargetEntityType;
        var property = address.FindProperty("ZipCode");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(20);
        property.GetColumnName().Should().Be("zip_code");
    }

    [Fact]
    public void Configure_PrimaryAddress_CountryProperty_ShouldBeRequiredWithMaxLength50()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var address = businessProfile.FindNavigation("PrimaryAddress")!.TargetEntityType;
        var property = address.FindProperty("Country");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(50);
        property.GetColumnName().Should().Be("country");
    }

    #endregion

    #region Documents Owned Collection Tests

    [Fact]
    public void Configure_Documents_ShouldBeOwnedCollection()
    {
        // Arrange
        var navigation = _entityType.FindNavigation("Documents");

        // Assert
        navigation.Should().NotBeNull();
        navigation!.ForeignKey.IsOwnership.Should().BeTrue();
        navigation.IsCollection.Should().BeTrue();
    }

    [Fact]
    public void Configure_Documents_ShouldMapToDocumentTable()
    {
        // Arrange
        var documentsType = _entityType.FindNavigation("Documents")!.TargetEntityType;

        // Assert
        documentsType.GetTableName().Should().Be("document");
        documentsType.GetSchema().Should().Be("providers");
    }

    [Fact]
    public void Configure_Documents_ShouldHaveCompositeKey()
    {
        // Arrange
        var documentsType = _entityType.FindNavigation("Documents")!.TargetEntityType;
        var primaryKey = documentsType.FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(2);
        primaryKey.Properties.Select(p => p.Name).Should().Contain("ProviderId");
        primaryKey.Properties.Select(p => p.Name).Should().Contain("Id");
    }

    [Fact]
    public void Configure_Documents_NumberProperty_ShouldBeRequiredWithMaxLength50()
    {
        // Arrange
        var documentsType = _entityType.FindNavigation("Documents")!.TargetEntityType;
        var property = documentsType.FindProperty("Number");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(50);
        property.GetColumnName().Should().Be("number");
    }

    [Fact]
    public void Configure_Documents_DocumentTypeProperty_ShouldHaveEnumConversion()
    {
        // Arrange
        var documentsType = _entityType.FindNavigation("Documents")!.TargetEntityType;
        var property = documentsType.FindProperty("DocumentType");

        // Assert
        property.Should().NotBeNull();
        property!.GetValueConverter().Should().NotBeNull();
        property.GetMaxLength().Should().Be(20);
        property.IsNullable.Should().BeFalse();
        property.GetColumnName().Should().Be("document_type");
    }

    [Fact]
    public void Configure_Documents_IsPrimaryProperty_ShouldHaveDefaultValueFalse()
    {
        // Arrange
        var documentsType = _entityType.FindNavigation("Documents")!.TargetEntityType;
        var property = documentsType.FindProperty("IsPrimary");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetColumnName().Should().Be("is_primary");
        property.GetDefaultValue().Should().Be(false);
    }

    [Fact]
    public void Configure_Documents_ProviderIdProperty_ShouldHaveCorrectColumnName()
    {
        // Arrange
        var documentsType = _entityType.FindNavigation("Documents")!.TargetEntityType;
        var property = documentsType.FindProperty("ProviderId");

        // Assert
        property.Should().NotBeNull();
        property!.GetColumnName().Should().Be("provider_id");
    }

    [Fact]
    public void Configure_Documents_IdProperty_ShouldHaveCorrectColumnName()
    {
        // Arrange
        var documentsType = _entityType.FindNavigation("Documents")!.TargetEntityType;
        var property = documentsType.FindProperty("Id");

        // Assert
        property.Should().NotBeNull();
        property!.GetColumnName().Should().Be("id");
    }

    [Fact]
    public void Configure_Documents_ShouldHaveUniqueIndexOnProviderIdAndDocumentType()
    {
        // Arrange
        var documentsType = _entityType.FindNavigation("Documents")!.TargetEntityType;
        var index = documentsType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 2 &&
                               i.Properties.Any(p => p.Name == "ProviderId") &&
                               i.Properties.Any(p => p.Name == "DocumentType"));

        // Assert
        index.Should().NotBeNull();
        index!.IsUnique.Should().BeTrue();
    }

    #endregion

    #region Qualifications Owned Collection Tests

    [Fact]
    public void Configure_Qualifications_ShouldBeOwnedCollection()
    {
        // Arrange
        var navigation = _entityType.FindNavigation("Qualifications");

        // Assert
        navigation.Should().NotBeNull();
        navigation!.ForeignKey.IsOwnership.Should().BeTrue();
        navigation.IsCollection.Should().BeTrue();
    }

    [Fact]
    public void Configure_Qualifications_ShouldMapToQualificationTable()
    {
        // Arrange
        var qualificationsType = _entityType.FindNavigation("Qualifications")!.TargetEntityType;

        // Assert
        qualificationsType.GetTableName().Should().Be("qualification");
        qualificationsType.GetSchema().Should().Be("providers");
    }

    [Fact]
    public void Configure_Qualifications_ShouldHaveCompositeKey()
    {
        // Arrange
        var qualificationsType = _entityType.FindNavigation("Qualifications")!.TargetEntityType;
        var primaryKey = qualificationsType.FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(2);
        primaryKey.Properties.Select(p => p.Name).Should().Contain("ProviderId");
        primaryKey.Properties.Select(p => p.Name).Should().Contain("Id");
    }

    [Fact]
    public void Configure_Qualifications_NameProperty_ShouldBeRequiredWithMaxLength200()
    {
        // Arrange
        var qualificationsType = _entityType.FindNavigation("Qualifications")!.TargetEntityType;
        var property = qualificationsType.FindProperty("Name");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(200);
        property.GetColumnName().Should().Be("name");
    }

    [Fact]
    public void Configure_Qualifications_DescriptionProperty_ShouldHaveMaxLength1000()
    {
        // Arrange
        var qualificationsType = _entityType.FindNavigation("Qualifications")!.TargetEntityType;
        var property = qualificationsType.FindProperty("Description");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(1000);
        property.GetColumnName().Should().Be("description");
    }

    [Fact]
    public void Configure_Qualifications_IssuingOrganizationProperty_ShouldHaveMaxLength200()
    {
        // Arrange
        var qualificationsType = _entityType.FindNavigation("Qualifications")!.TargetEntityType;
        var property = qualificationsType.FindProperty("IssuingOrganization");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(200);
        property.GetColumnName().Should().Be("issuing_organization");
    }

    [Fact]
    public void Configure_Qualifications_IssueDateProperty_ShouldHaveCorrectColumnName()
    {
        // Arrange
        var qualificationsType = _entityType.FindNavigation("Qualifications")!.TargetEntityType;
        var property = qualificationsType.FindProperty("IssueDate");

        // Assert
        property.Should().NotBeNull();
        property!.GetColumnName().Should().Be("issue_date");
    }

    [Fact]
    public void Configure_Qualifications_ExpirationDateProperty_ShouldHaveCorrectColumnName()
    {
        // Arrange
        var qualificationsType = _entityType.FindNavigation("Qualifications")!.TargetEntityType;
        var property = qualificationsType.FindProperty("ExpirationDate");

        // Assert
        property.Should().NotBeNull();
        property!.GetColumnName().Should().Be("expiration_date");
    }

    [Fact]
    public void Configure_Qualifications_DocumentNumberProperty_ShouldHaveMaxLength50()
    {
        // Arrange
        var qualificationsType = _entityType.FindNavigation("Qualifications")!.TargetEntityType;
        var property = qualificationsType.FindProperty("DocumentNumber");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(50);
        property.GetColumnName().Should().Be("document_number");
    }

    [Fact]
    public void Configure_Qualifications_ProviderIdProperty_ShouldHaveCorrectColumnName()
    {
        // Arrange
        var qualificationsType = _entityType.FindNavigation("Qualifications")!.TargetEntityType;
        var property = qualificationsType.FindProperty("ProviderId");

        // Assert
        property.Should().NotBeNull();
        property!.GetColumnName().Should().Be("provider_id");
    }

    [Fact]
    public void Configure_Qualifications_IdProperty_ShouldHaveCorrectColumnName()
    {
        // Arrange
        var qualificationsType = _entityType.FindNavigation("Qualifications")!.TargetEntityType;
        var property = qualificationsType.FindProperty("Id");

        // Assert
        property.Should().NotBeNull();
        property!.GetColumnName().Should().Be("id");
    }

    #endregion

    #region Services Relationship Tests

    [Fact]
    public void Configure_Services_ShouldHaveOneToManyRelationship()
    {
        // Arrange
        var navigation = _entityType.FindNavigation("Services");

        // Assert
        navigation.Should().NotBeNull();
        navigation!.IsCollection.Should().BeTrue();
        navigation.ForeignKey.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }

    #endregion

    #region Index Tests

    [Fact]
    public void Configure_ShouldHaveUniqueIndexOnUserId()
    {
        // Arrange
        var index = _entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 1 && i.Properties.First().Name == "UserId");

        // Assert
        index.Should().NotBeNull();
        index!.IsUnique.Should().BeTrue();
        index.GetDatabaseName().Should().Be("ix_providers_user_id");
    }

    [Fact]
    public void Configure_ShouldHaveIndexOnName()
    {
        // Arrange
        var index = _entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 1 && i.Properties.First().Name == "Name");

        // Assert
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be("ix_providers_name");
    }

    [Fact]
    public void Configure_ShouldHaveIndexOnType()
    {
        // Arrange
        var index = _entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 1 && i.Properties.First().Name == "Type");

        // Assert
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be("ix_providers_type");
    }

    [Fact]
    public void Configure_ShouldHaveIndexOnStatus()
    {
        // Arrange
        var index = _entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 1 && i.Properties.First().Name == "Status");

        // Assert
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be("ix_providers_status");
    }

    [Fact]
    public void Configure_ShouldHaveIndexOnVerificationStatus()
    {
        // Arrange
        var index = _entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 1 && i.Properties.First().Name == "VerificationStatus");

        // Assert
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be("ix_providers_verification_status");
    }

    [Fact]
    public void Configure_ShouldHaveIndexOnIsDeleted()
    {
        // Arrange
        var index = _entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 1 && i.Properties.First().Name == "IsDeleted");

        // Assert
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be("ix_providers_is_deleted");
    }

    [Fact]
    public void Configure_ShouldHaveSixIndexes()
    {
        // Assert - 6 indexes on Provider entity itself:
        // 1. UserId (unique)
        // 2. Name
        // 3. Type
        // 4. Status
        // 5. VerificationStatus
        // 6. IsDeleted
        _entityType.GetIndexes().Should().HaveCount(6);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Configure_AllOwnedEntities_ShouldBeProperlyConfigured()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile");
        var documents = _entityType.FindNavigation("Documents");
        var qualifications = _entityType.FindNavigation("Qualifications");

        // Assert
        businessProfile.Should().NotBeNull();
        businessProfile!.ForeignKey.IsOwnership.Should().BeTrue();

        documents.Should().NotBeNull();
        documents!.ForeignKey.IsOwnership.Should().BeTrue();
        documents.IsCollection.Should().BeTrue();

        qualifications.Should().NotBeNull();
        qualifications!.ForeignKey.IsOwnership.Should().BeTrue();
        qualifications.IsCollection.Should().BeTrue();
    }

    [Fact]
    public void Configure_AllRequiredProperties_ShouldBeNonNullable()
    {
        // Arrange
        var requiredProperties = new[] { "Id", "Name", "Type", "Status", "VerificationStatus", "IsDeleted", "CreatedAt" };

        // Assert
        foreach (var propertyName in requiredProperties)
        {
            var property = _entityType.FindProperty(propertyName);
            property.Should().NotBeNull($"Property {propertyName} should exist");
            property!.IsNullable.Should().BeFalse($"Property {propertyName} should be required");
        }
    }

    [Fact]
    public void Configure_AllNullableProperties_ShouldBeNullable()
    {
        // Arrange
        var nullableProperties = new[] { "DeletedAt", "UpdatedAt", "SuspensionReason", "RejectionReason" };

        // Assert
        foreach (var propertyName in nullableProperties)
        {
            var property = _entityType.FindProperty(propertyName);
            property.Should().NotBeNull($"Property {propertyName} should exist");
            property!.IsNullable.Should().BeTrue($"Property {propertyName} should be nullable");
        }
    }

    [Fact]
    public void Configure_AllEnumProperties_ShouldHaveValueConverters()
    {
        // Arrange
        var enumProperties = new[] { "Type", "Status", "VerificationStatus" };

        // Assert
        foreach (var propertyName in enumProperties)
        {
            var property = _entityType.FindProperty(propertyName);
            property.Should().NotBeNull($"Property {propertyName} should exist");
            property!.GetValueConverter().Should().NotBeNull($"Property {propertyName} should have a value converter");
        }
    }

    [Fact]
    public void Configure_NestedOwnedEntities_ShouldBeProperlyConfigured()
    {
        // Arrange
        var businessProfile = _entityType.FindNavigation("BusinessProfile")!.TargetEntityType;
        var contactInfo = businessProfile.FindNavigation("ContactInfo");
        var primaryAddress = businessProfile.FindNavigation("PrimaryAddress");

        // Assert
        contactInfo.Should().NotBeNull();
        contactInfo!.ForeignKey.IsOwnership.Should().BeTrue();

        primaryAddress.Should().NotBeNull();
        primaryAddress!.ForeignKey.IsOwnership.Should().BeTrue();
    }

    #endregion

    // Test DbContext for ProviderConfiguration testing
    private sealed class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<Provider> Providers => Set<Provider>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProviderConfiguration());
            modelBuilder.ApplyConfiguration(new ProviderServiceConfiguration());
        }
    }
}
