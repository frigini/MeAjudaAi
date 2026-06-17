using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Mappers;
using MeAjudaAi.Modules.Documents.Domain.Enums;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "Documents")]
[Trait("Layer", "Application")]
public class DocumentModuleMapperTests
{
    [Fact]
    public void ToModuleDto_ShouldMapAllProperties()
    {
        // Arrange
        var dto = new DocumentDto(
            Id: Guid.NewGuid(),
            ProviderId: Guid.NewGuid(),
            DocumentType: EDocumentType.IdentityDocument,
            FileName: "test.pdf",
            FileUrl: "https://storage.example.com/test.pdf",
            Status: EDocumentStatus.Verified,
            UploadedAt: new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            VerifiedAt: new DateTime(2025, 1, 16, 14, 0, 0, DateTimeKind.Utc),
            RejectionReason: null,
            OcrData: null);

        // Act
        var result = dto.ToModuleDto();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(dto.Id);
        result.ProviderId.Should().Be(dto.ProviderId);
        result.DocumentType.Should().Be("IdentityDocument");
        result.FileName.Should().Be("test.pdf");
        result.FileUrl.Should().Be("https://storage.example.com/test.pdf");
        result.Status.Should().Be("Verified");
        result.UploadedAt.Should().Be(dto.UploadedAt);
        result.VerifiedAt.Should().Be(dto.VerifiedAt);
        result.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void ToModuleDto_WithNullVerifiedAt_ShouldMapCorrectly()
    {
        // Arrange
        var dto = new DocumentDto(
            Id: Guid.NewGuid(),
            ProviderId: Guid.NewGuid(),
            DocumentType: EDocumentType.ProofOfResidence,
            FileName: "proof.pdf",
            FileUrl: "https://storage.example.com/proof.pdf",
            Status: EDocumentStatus.PendingVerification,
            UploadedAt: DateTime.UtcNow,
            VerifiedAt: null,
            RejectionReason: null,
            OcrData: null);

        // Act
        var result = dto.ToModuleDto();

        // Assert
        result.VerifiedAt.Should().BeNull();
        result.Status.Should().Be("PendingVerification");
    }

    [Fact]
    public void ToModuleDto_WithRejectionReason_ShouldMapCorrectly()
    {
        // Arrange
        var dto = new DocumentDto(
            Id: Guid.NewGuid(),
            ProviderId: Guid.NewGuid(),
            DocumentType: EDocumentType.CriminalRecord,
            FileName: "criminal.pdf",
            FileUrl: "https://storage.example.com/criminal.pdf",
            Status: EDocumentStatus.Rejected,
            UploadedAt: DateTime.UtcNow,
            VerifiedAt: DateTime.UtcNow,
            RejectionReason: "Document is blurry",
            OcrData: null);

        // Act
        var result = dto.ToModuleDto();

        // Assert
        result.RejectionReason.Should().Be("Document is blurry");
        result.Status.Should().Be("Rejected");
    }

    [Theory]
    [InlineData(EDocumentType.IdentityDocument, "IdentityDocument")]
    [InlineData(EDocumentType.ProofOfResidence, "ProofOfResidence")]
    [InlineData(EDocumentType.CriminalRecord, "CriminalRecord")]
    [InlineData(EDocumentType.Other, "Other")]
    public void ToModuleDto_ShouldConvertDocumentTypeToString(EDocumentType documentType, string expected)
    {
        // Arrange
        var dto = new DocumentDto(
            Id: Guid.NewGuid(),
            ProviderId: Guid.NewGuid(),
            DocumentType: documentType,
            FileName: "test.pdf",
            FileUrl: "https://storage.example.com/test.pdf",
            Status: EDocumentStatus.Uploaded,
            UploadedAt: DateTime.UtcNow,
            VerifiedAt: null,
            RejectionReason: null,
            OcrData: null);

        // Act
        var result = dto.ToModuleDto();

        // Assert
        result.DocumentType.Should().Be(expected);
    }

    [Theory]
    [InlineData(EDocumentStatus.Uploaded, "Uploaded")]
    [InlineData(EDocumentStatus.PendingVerification, "PendingVerification")]
    [InlineData(EDocumentStatus.Verified, "Verified")]
    [InlineData(EDocumentStatus.Rejected, "Rejected")]
    [InlineData(EDocumentStatus.Failed, "Failed")]
    public void ToModuleDto_ShouldConvertStatusToString(EDocumentStatus status, string expected)
    {
        // Arrange
        var dto = new DocumentDto(
            Id: Guid.NewGuid(),
            ProviderId: Guid.NewGuid(),
            DocumentType: EDocumentType.IdentityDocument,
            FileName: "test.pdf",
            FileUrl: "https://storage.example.com/test.pdf",
            Status: status,
            UploadedAt: DateTime.UtcNow,
            VerifiedAt: null,
            RejectionReason: null,
            OcrData: null);

        // Act
        var result = dto.ToModuleDto();

        // Assert
        result.Status.Should().Be(expected);
    }

    [Fact]
    public void ToModuleDto_CalledMultipleTimes_ShouldReturnSameResult()
    {
        // Arrange
        var dto = new DocumentDto(
            Id: Guid.NewGuid(),
            ProviderId: Guid.NewGuid(),
            DocumentType: EDocumentType.IdentityDocument,
            FileName: "test.pdf",
            FileUrl: "https://storage.example.com/test.pdf",
            Status: EDocumentStatus.Verified,
            UploadedAt: DateTime.UtcNow,
            VerifiedAt: DateTime.UtcNow,
            RejectionReason: null,
            OcrData: null);

        // Act
        var result1 = dto.ToModuleDto();
        var result2 = dto.ToModuleDto();

        // Assert
        result1.Should().BeEquivalentTo(result2);
    }
}
