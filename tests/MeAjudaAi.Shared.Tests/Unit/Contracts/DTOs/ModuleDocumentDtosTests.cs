using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Shared.Contracts.Modules.Documents.DTOs;

namespace MeAjudaAi.Shared.Tests.Unit.Contracts.DTOs;

/// <summary>
/// Testes unitários para DTOs do módulo Documents em Shared.Contracts
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Contracts")]
public class ModuleDocumentDtosTests
{
    #region ModuleDocumentDto Tests

    [Fact]
    public void ModuleDocumentDto_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var dto = new ModuleDocumentDto
        {
            Id = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            DocumentType = "CPF",
            FileName = "documento.pdf",
            FileUrl = "https://storage.example.com/documento.pdf",
            Status = "Verified",
            UploadedAt = DateTime.UtcNow,
            VerifiedAt = DateTime.UtcNow.AddHours(1),
            RejectionReason = null,
            OcrData = null
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ModuleDocumentDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeEquivalentTo(dto, options => options
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1)))
            .WhenTypeIs<DateTime>());
    }

    [Fact]
    public void ModuleDocumentDto_WithRejectedStatus_ShouldIncludeRejectionReason()
    {
        // Arrange
        var dto = new ModuleDocumentDto
        {
            Id = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            DocumentType = "RG",
            FileName = "documento.pdf",
            FileUrl = "https://storage.example.com/documento.pdf",
            Status = "Rejected",
            UploadedAt = DateTime.UtcNow,
            VerifiedAt = DateTime.UtcNow.AddHours(1),
            RejectionReason = "Documento ilegível",
            OcrData = null
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ModuleDocumentDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Status.Should().Be("Rejected");
        deserialized.RejectionReason.Should().Be("Documento ilegível");
        deserialized.VerifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void ModuleDocumentDto_WithOcrData_ShouldSerializeOcrData()
    {
        // Arrange
        var ocrData = "{\"name\":\"João Silva\",\"cpf\":\"12345678901\"}";
        var dto = new ModuleDocumentDto
        {
            Id = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            DocumentType = "CPF",
            FileName = "cpf.pdf",
            FileUrl = "https://storage.example.com/cpf.pdf",
            Status = "Verified",
            UploadedAt = DateTime.UtcNow,
            VerifiedAt = DateTime.UtcNow,
            RejectionReason = null,
            OcrData = ocrData
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ModuleDocumentDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.OcrData.Should().Be(ocrData);
    }

    [Theory]
    [InlineData("CPF")]
    [InlineData("CNPJ")]
    [InlineData("RG")]
    [InlineData("CNH")]
    public void ModuleDocumentDto_ShouldSupportMultipleDocumentTypes(string documentType)
    {
        // Arrange & Act
        var dto = new ModuleDocumentDto
        {
            Id = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            DocumentType = documentType,
            FileName = $"{documentType}.pdf",
            FileUrl = $"https://storage.example.com/{documentType}.pdf",
            Status = "Uploaded",
            UploadedAt = DateTime.UtcNow
        };

        // Assert
        dto.DocumentType.Should().Be(documentType);
    }

    [Theory]
    [InlineData("Uploaded")]
    [InlineData("Pending")]
    [InlineData("Verified")]
    [InlineData("Rejected")]
    public void ModuleDocumentDto_ShouldSupportAllStatuses(string status)
    {
        // Arrange & Act
        var dto = new ModuleDocumentDto
        {
            Id = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            DocumentType = "CPF",
            FileName = "doc.pdf",
            FileUrl = "https://storage.example.com/doc.pdf",
            Status = status,
            UploadedAt = DateTime.UtcNow
        };

        // Assert
        dto.Status.Should().Be(status);
    }

    #endregion

    #region ModuleDocumentStatusDto Tests

    [Fact]
    public void ModuleDocumentStatusDto_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var dto = new ModuleDocumentStatusDto
        {
            DocumentId = Guid.NewGuid(),
            Status = "processing",
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ModuleDocumentStatusDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeEquivalentTo(dto, options => options
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1)))
            .WhenTypeIs<DateTime>());
    }

    [Fact]
    public void ModuleDocumentStatusDto_ShouldContainOnlyEssentialFields()
    {
        // Arrange
        var dto = new ModuleDocumentStatusDto
        {
            DocumentId = Guid.NewGuid(),
            Status = "completed",
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert - Should have exactly 3 properties
        jsonElement.EnumerateObject().Should().HaveCount(3);
        jsonElement.TryGetProperty("DocumentId", out _).Should().BeTrue();
        jsonElement.TryGetProperty("Status", out _).Should().BeTrue();
        jsonElement.TryGetProperty("UpdatedAt", out _).Should().BeTrue();
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("processing")]
    [InlineData("completed")]
    [InlineData("failed")]
    public void ModuleDocumentStatusDto_ShouldSupportValidStatuses(string status)
    {
        // Arrange & Act
        var dto = new ModuleDocumentStatusDto
        {
            DocumentId = Guid.NewGuid(),
            Status = status,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        dto.Status.Should().Be(status);
    }

    #endregion

    #region DocumentStatusCountDto Tests

    [Fact]
    public void DocumentStatusCountDto_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var dto = new DocumentStatusCountDto
        {
            Total = 100,
            Pending = 42,
            Verified = 35,
            Rejected = 18,
            Uploading = 5
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<DocumentStatusCountDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public void DocumentStatusCountDto_WithAllZeroCounts_ShouldSerialize()
    {
        // Arrange
        var dto = new DocumentStatusCountDto
        {
            Total = 0,
            Pending = 0,
            Verified = 0,
            Rejected = 0,
            Uploading = 0
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<DocumentStatusCountDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Total.Should().Be(0);
        deserialized.Pending.Should().Be(0);
        deserialized.Verified.Should().Be(0);
        deserialized.Rejected.Should().Be(0);
        deserialized.Uploading.Should().Be(0);
    }

    [Fact]
    public void DocumentStatusCountDto_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        var dto = new DocumentStatusCountDto
        {
            Total = 100,
            Pending = 25,
            Verified = 50,
            Rejected = 15,
            Uploading = 10
        };

        // Assert - Total should match sum of individual counts
        (dto.Pending + dto.Verified + dto.Rejected + dto.Uploading).Should().Be(dto.Total);
    }

    [Fact]
    public void DocumentStatusCountDto_ShouldSerializeWithCamelCase()
    {
        // Arrange
        var dto = new DocumentStatusCountDto
        {
            Total = 100,
            Pending = 25,
            Verified = 50,
            Rejected = 15,
            Uploading = 10
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var json = JsonSerializer.Serialize(dto, options);

        // Assert
        json.Should().Contain("\"total\":100");
        json.Should().Contain("\"pending\":25");
        json.Should().Contain("\"verified\":50");
        json.Should().Contain("\"rejected\":15");
        json.Should().Contain("\"uploading\":10");
    }

    #endregion
}
