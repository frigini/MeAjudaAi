using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "API")]
public class RequestMapperExtensionsTests
{
    [Fact]
    public void ToCommand_WithCreateProviderRequest_ShouldMapAllProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var contactInfo = new ContactInfoDto("test@example.com", "+5511999999999", "https://example.com");
        var address = new AddressDto("Main St", "123", "Suite 100", "Downtown", "São Paulo", "SP", "01310-000", "Brazil");
        var businessProfile = new BusinessProfileDto("Legal Name", "Fantasy", "Description", contactInfo, address);
        var request = new CreateProviderRequest
        {
            UserId = userId,
            Name = "Test Provider",
            Type = EProviderType.Company,
            BusinessProfile = businessProfile
        };

        // Act
        var command = request.ToCommand();

        // Assert
        command.Should().NotBeNull();
        command.UserId.Should().Be(userId);
        command.Name.Should().Be("Test Provider");
        command.Type.Should().Be(EProviderType.Company);
        command.BusinessProfile.Should().BeSameAs(businessProfile);
    }

    [Fact]
    public void ToCommand_WithCreateProviderRequestNullBusinessProfile_ShouldThrowArgumentNullException()
    {
        // Arrange
        var request = new CreateProviderRequest
        {
            UserId = Guid.NewGuid(),
            Name = "Test",
            Type = EProviderType.Individual,
            BusinessProfile = null
        };

        // Act
        var act = () => request.ToCommand();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToCommand_WithUpdateProviderProfileRequest_ShouldMapAllPropertiesIncludingProviderId()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var contactInfo = new ContactInfoDto("updated@example.com", null, null);
        var address = new AddressDto("New St", "456", null, "Uptown", "Rio de Janeiro", "RJ", "20000-000", "Brazil");
        var businessProfile = new BusinessProfileDto("Updated Name", null, null, contactInfo, address);
        var services = new List<ProviderServiceDto>
        {
            new(Guid.NewGuid(), "Service 1"),
            new(Guid.NewGuid(), "Service 2")
        };
        var request = new UpdateProviderProfileRequest
        {
            Name = "Updated Provider",
            BusinessProfile = businessProfile,
            Services = services
        };

        // Act
        var command = request.ToCommand(providerId);

        // Assert
        command.Should().NotBeNull();
        command.ProviderId.Should().Be(providerId);
        command.Name.Should().Be("Updated Provider");
        command.BusinessProfile.Should().BeSameAs(businessProfile);
        command.Services.Should().BeSameAs(services);
    }

    [Fact]
    public void ToCommand_WithUpdateProviderProfileRequestNullServices_ShouldMapNullServices()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var contactInfo = new ContactInfoDto("test@example.com", null, null);
        var address = new AddressDto("Main St", "123", null, "Downtown", "São Paulo", "SP", "01310-000", "Brazil");
        var businessProfile = new BusinessProfileDto("Name", null, null, contactInfo, address);
        var request = new UpdateProviderProfileRequest
        {
            Name = "Provider",
            BusinessProfile = businessProfile,
            Services = null
        };

        // Act
        var command = request.ToCommand(providerId);

        // Assert
        command.Services.Should().BeNull();
    }

    [Fact]
    public void ToCommand_WithAddDocumentRequest_ShouldMapAllPropertiesIncludingProviderId()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new AddDocumentRequest(
            Number: "12345678900",
            DocumentType: EDocumentType.CPF,
            FileName: "doc.pdf",
            FileUrl: "https://storage/doc.pdf");

        // Act
        var command = request.ToCommand(providerId);

        // Assert
        command.Should().NotBeNull();
        command.ProviderId.Should().Be(providerId);
        command.DocumentNumber.Should().Be("12345678900");
        command.DocumentType.Should().Be(EDocumentType.CPF);
        command.FileName.Should().Be("doc.pdf");
        command.FileUrl.Should().Be("https://storage/doc.pdf");
    }

    [Fact]
    public void ToCommand_WithAddDocumentRequestOptionalFieldsNull_ShouldMapNulls()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new AddDocumentRequest(
            Number: "12345678900",
            DocumentType: EDocumentType.CPF);

        // Act
        var command = request.ToCommand(providerId);

        // Assert
        command.FileName.Should().BeNull();
        command.FileUrl.Should().BeNull();
    }

    [Fact]
    public void ToCommand_WithUpdateVerificationStatusRequest_ShouldMapAllPropertiesIncludingProviderId()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new UpdateVerificationStatusRequest
        {
            Status = EVerificationStatus.Verified
        };

        // Act
        var command = request.ToCommand(providerId);

        // Assert
        command.Should().NotBeNull();
        command.ProviderId.Should().Be(providerId);
        command.Status.Should().Be(EVerificationStatus.Verified);
    }

    [Fact]
    public void ToCommand_WithRequireBasicInfoCorrectionRequest_ShouldMapAllProperties()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var requestedBy = "admin@test.com";
        var request = new RequireBasicInfoCorrectionRequest
        {
            Reason = "Missing document"
        };

        // Act
        var command = request.ToCommand(providerId, requestedBy);

        // Assert
        command.Should().NotBeNull();
        command.ProviderId.Should().Be(providerId);
        command.Reason.Should().Be("Missing document");
        command.RequestedBy.Should().Be("admin@test.com");
    }

    [Fact]
    public void ToQuery_WithProviderId_ShouldMapToGetProviderByIdQuery()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        // Act
        var query = providerId.ToQuery();

        // Assert
        query.Should().NotBeNull();
        query.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public void ToUserQuery_WithUserId_ShouldMapToGetProviderByUserIdQuery()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var query = userId.ToUserQuery();

        // Assert
        query.Should().NotBeNull();
        query.UserId.Should().Be(userId);
    }

    [Fact]
    public void ToCityQuery_WithCity_ShouldMapToGetProvidersByCityQuery()
    {
        // Arrange
        var city = "São Paulo";

        // Act
        var query = city.ToCityQuery();

        // Assert
        query.Should().NotBeNull();
        query.City.Should().Be("São Paulo");
    }

    [Fact]
    public void ToStateQuery_WithState_ShouldMapToGetProvidersByStateQuery()
    {
        // Arrange
        var state = "SP";

        // Act
        var query = state.ToStateQuery();

        // Assert
        query.Should().NotBeNull();
        query.State.Should().Be("SP");
    }

    [Fact]
    public void ToTypeQuery_WithType_ShouldMapToGetProvidersByTypeQuery()
    {
        // Arrange
        var type = EProviderType.Company;

        // Act
        var query = type.ToTypeQuery();

        // Assert
        query.Should().NotBeNull();
        query.Type.Should().Be(EProviderType.Company);
    }

    [Fact]
    public void ToVerificationStatusQuery_WithStatus_ShouldMapToGetProvidersByVerificationStatusQuery()
    {
        // Arrange
        var status = EVerificationStatus.Pending;

        // Act
        var query = status.ToVerificationStatusQuery();

        // Assert
        query.Should().NotBeNull();
        query.Status.Should().Be(EVerificationStatus.Pending);
    }

    [Fact]
    public void ToDeleteCommand_WithProviderId_ShouldMapToDeleteProviderCommand()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        // Act
        var command = providerId.ToDeleteCommand();

        // Assert
        command.Should().NotBeNull();
        command.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public void ToDeleteCommand_WithEmptyGuid_ShouldMapEmptyGuid()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        var command = emptyId.ToDeleteCommand();

        // Assert
        command.ProviderId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ToRemoveDocumentCommand_WithProviderIdAndDocumentType_ShouldMapToRemoveDocumentCommand()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        // Act
        var command = providerId.ToRemoveDocumentCommand(EDocumentType.CPF);

        // Assert
        command.Should().NotBeNull();
        command.ProviderId.Should().Be(providerId);
        command.DocumentType.Should().Be(EDocumentType.CPF);
    }

    [Fact]
    public void ToProvidersQuery_WithGetProvidersRequest_ShouldMapAllProperties()
    {
        // Arrange
        var request = new GetProvidersRequest
        {
            Name = "Test",
            Type = (int)EProviderType.Company,
            VerificationStatus = (int)EVerificationStatus.Verified,
            PageNumber = 2,
            PageSize = 25
        };

        // Act
        var query = request.ToProvidersQuery();

        // Assert
        query.Should().NotBeNull();
        query.Page.Should().Be(2);
        query.PageSize.Should().Be(25);
        query.Name.Should().Be("Test");
        query.Type.Should().Be((int)EProviderType.Company);
        query.VerificationStatus.Should().Be((int)EVerificationStatus.Verified);
    }

    [Fact]
    public void ToProvidersQuery_WithDefaultValues_ShouldMapDefaults()
    {
        // Arrange
        var request = new GetProvidersRequest();

        // Act
        var query = request.ToProvidersQuery();

        // Assert
        query.Should().NotBeNull();
        query.Page.Should().Be(1);
        query.PageSize.Should().Be(20);
        query.Name.Should().BeNull();
        query.Type.Should().BeNull();
        query.VerificationStatus.Should().BeNull();
    }
}
