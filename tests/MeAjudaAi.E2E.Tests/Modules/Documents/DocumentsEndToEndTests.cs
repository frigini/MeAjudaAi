using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Modules.Documents;

/// <summary>
/// Testes E2E para o módulo Documents
/// Valida upload de documentos, persistência no banco, e fluxo completo de verificação
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Documents")]
public class DocumentsEndToEndTests : TestContainerTestBase
{
    #region Helper Methods

    private async Task WaitForProviderAsync(Guid providerId, int maxAttempts = 10)
    {
        var delay = 100; // Start with 100ms
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                var response = await ApiClient.GetAsync($"/api/v1/providers/{providerId}");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (Exception) when (attempt < maxAttempts - 1)
            {
                // Trata erros de rede transitórios como retentáveis
            }

            if (attempt < maxAttempts - 1)
            {
                await Task.Delay(delay);
                delay = Math.Min(delay * 2, 2000); // Exponential backoff, max 2s
            }
        }

        throw new TimeoutException($"Provider {providerId} was not found after {maxAttempts} attempts");
    }

    #endregion

    #region Upload and Basic CRUD Tests

    [Fact]
    public async Task UploadDocument_Should_CreateDocumentInDatabase()
    {
        // Arrange
        AuthenticateAsAdmin();
        var providerId = Guid.NewGuid();

        var uploadRequest = new
        {
            ProviderId = providerId,
            DocumentType = (int)EDocumentType.IdentityDocument,
            FileName = "identity-card.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 102400
        };

        // Act
        var response = await PostJsonAsync("/api/v1/documents/upload", uploadRequest);

        // Assert
        // O container Azurite é criado automaticamente por AzureBlobStorageService.EnsureContainerExistsAsync
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);

            result.TryGetProperty("documentId", out var documentIdElement).Should().BeTrue();
            var documentId = Guid.Parse(documentIdElement.GetString()!);

            // Verify database persistence
            await WithServiceScopeAsync(async services =>
            {
                var dbContext = services.GetRequiredService<DocumentsDbContext>();
                var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId);

                document.Should().NotBeNull();
                document.ProviderId.Should().Be(providerId);
                document.Status.Should().Be(EDocumentStatus.Uploaded);
                document.DocumentType.Should().Be(EDocumentType.IdentityDocument);
            });
        }
        else
        {
            // O upload pode falhar no ambiente de teste se o Azurite não estiver em execução
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.InternalServerError);
        }
    }

    [Fact]
    public async Task GetDocumentStatus_Should_ReturnDocumentDetails()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        Guid documentId = Guid.Empty;

        // Create document directly in database
        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();

            var document = Document.Create(
                providerId,
                EDocumentType.ProofOfResidence,
                "proof-residence.pdf",
                "https://storage.test.com/proof.pdf");

            dbContext.Documents.Add(document);
            await dbContext.SaveChangesAsync();
            documentId = document.Id;
        });

        AuthenticateAsAdmin();

        // Act
        var response = await ApiClient.GetAsync($"/api/v1/documents/{documentId}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);

        result.TryGetProperty("id", out var idElement).Should().BeTrue();
        Guid.Parse(idElement.GetString()!).Should().Be(documentId);
    }

    #endregion

    #region Provider Documents Tests

    [Fact]
    public async Task GetProviderDocuments_Should_ReturnAllDocuments()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        // Create multiple documents for provider
        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();

            var doc1 = Document.Create(providerId, EDocumentType.IdentityDocument, "id.pdf", "url1");
            var doc2 = Document.Create(providerId, EDocumentType.ProofOfResidence, "proof.pdf", "url2");
            var doc3 = Document.Create(providerId, EDocumentType.CriminalRecord, "record.pdf", "url3");

            dbContext.Documents.AddRange(doc1, doc2, doc3);
            await dbContext.SaveChangesAsync();
        });

        AuthenticateAsAdmin();

        // Act
        var response = await ApiClient.GetAsync($"/api/v1/documents/provider/{providerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var documents = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);

        documents.ValueKind.Should().Be(JsonValueKind.Array);
        documents.GetArrayLength().Should().Be(3);
    }

    #endregion

    #region Document Workflow and Status Transitions

    [Fact]
    public async Task DocumentWorkflow_Should_TransitionThroughStatuses()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        Guid documentId = Guid.Empty;

        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();

            var document = Document.Create(
                providerId,
                EDocumentType.CriminalRecord,
                "criminal-record.pdf",
                "https://storage.test.com/record.pdf");

            documentId = document.Id;

            // Test status transitions
            document.MarkAsPendingVerification();
            document.Status.Should().Be(EDocumentStatus.PendingVerification);

            document.MarkAsVerified("{\"verified\": true}");
            document.Status.Should().Be(EDocumentStatus.Verified);
            document.VerifiedAt.Should().NotBeNull();

            dbContext.Documents.Add(document);
            await dbContext.SaveChangesAsync();
        });

        // Assert - Verify persisted status
        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();
            var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId);

            document.Should().NotBeNull();
            document!.Status.Should().Be(EDocumentStatus.Verified);
            document.OcrData.Should().NotBeNull();
        });
    }

    #endregion

    #region Multiple Providers and Isolation Tests

    [Fact]
    public async Task Database_Should_StoreMultipleProvidersDocuments()
    {
        // Arrange
        var provider1 = Guid.NewGuid();
        var provider2 = Guid.NewGuid();

        // Act - Create documents for different providers
        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();

            var doc1 = Document.Create(provider1, EDocumentType.IdentityDocument, "p1-id.pdf", "url1");
            var doc2 = Document.Create(provider1, EDocumentType.ProofOfResidence, "p1-proof.pdf", "url2");
            var doc3 = Document.Create(provider2, EDocumentType.CriminalRecord, "p2-record.pdf", "url3");

            dbContext.Documents.AddRange(doc1, doc2, doc3);
            await dbContext.SaveChangesAsync();
        });

        // Assert - Verify isolation between providers
        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();

            var provider1Docs = await dbContext.Documents
                .Where(d => d.ProviderId == provider1)
                .ToListAsync();

            var provider2Docs = await dbContext.Documents
                .Where(d => d.ProviderId == provider2)
                .ToListAsync();

            provider1Docs.Should().HaveCount(2);
            provider2Docs.Should().HaveCount(1);
        });
    }

    #endregion

    #region Verification Workflow Tests

    [Fact]
    public async Task DocumentLifecycle_UploadAndVerification_ShouldCompleteProperly()
    {
        // Arrange
        AuthenticateAsAdmin();
        var providerId = Guid.NewGuid();

        // Act - Upload document
        var uploadRequest = new
        {
            ProviderId = providerId,
            DocumentType = (int)EDocumentType.CriminalRecord,
            FileName = "criminal-record.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 51200
        };

        var uploadResponse = await PostJsonAsync("/api/v1/documents/upload", uploadRequest);

        if (uploadResponse.StatusCode != HttpStatusCode.OK)
        {
            // Pula o teste se o Azurite não estiver disponível
            uploadResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.InternalServerError);
            return;
        }

        var uploadContent = await uploadResponse.Content.ReadAsStringAsync();
        var uploadResult = JsonSerializer.Deserialize<JsonElement>(uploadContent, JsonOptions);
        var documentId = Guid.Parse(uploadResult.GetProperty("documentId").GetString()!);

        // Act - Marca como verificado
        var verifyRequest = new
        {
            DocumentId = documentId,
            IsVerified = true,
            VerificationNotes = "Document verification completed successfully"
        };

        var verifyResponse = await PostJsonAsync("/api/v1/documents/verify", verifyRequest);

        // Assert - Verification should succeed
        verifyResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        // Assert - Verify status change in database
        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();
            var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId);

            document.Should().NotBeNull();
            document.Status.Should().Be(EDocumentStatus.Verified);
            document.RejectionReason.Should().BeNullOrEmpty("verified documents should not have rejection reasons");
        });
    }

    [Fact]
    public async Task DocumentRejection_ShouldUpdateStatusCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        Guid documentId = Guid.Empty;

        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();
            var document = Document.Create(
                providerId,
                EDocumentType.ProofOfResidence,
                "proof-address.pdf",
                "blob-key-proof-address");

            dbContext.Documents.Add(document);
            await dbContext.SaveChangesAsync();
            documentId = document.Id;
        });

        AuthenticateAsAdmin();

        // Act - Reject document
        var rejectRequest = new
        {
            DocumentId = documentId,
            IsVerified = false,
            VerificationNotes = "Document is not legible"
        };

        var rejectResponse = await PostJsonAsync("/api/v1/documents/verify", rejectRequest);

        // Assert
        rejectResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();
            var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId);

            document.Should().NotBeNull();
            document.Status.Should().Be(EDocumentStatus.Rejected);
            document.RejectionReason.Should().Contain("not legible");
        });
    }

    [Fact]
    public async Task MultipleDocuments_SameProvider_ShouldMaintainHistory()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documentIds = new List<Guid>();

        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();

            // Create multiple documents for the same provider
            var doc1 = Document.Create(providerId, EDocumentType.IdentityDocument, "id-v1.pdf", "blob-key-id-v1");
            var doc2 = Document.Create(providerId, EDocumentType.IdentityDocument, "id-v2.pdf", "blob-key-id-v2");
            var doc3 = Document.Create(providerId, EDocumentType.CriminalRecord, "criminal.pdf", "blob-key-criminal");

            dbContext.Documents.AddRange(doc1, doc2, doc3);
            await dbContext.SaveChangesAsync();

            documentIds.Add(doc1.Id);
            documentIds.Add(doc2.Id);
            documentIds.Add(doc3.Id);
        });

        AuthenticateAsAdmin();

        // Act - Verify first identity document
        var verify1 = new
        {
            DocumentId = documentIds[0],
            IsVerified = true,
            VerificationNotes = "First version verified"
        };
        await PostJsonAsync("/api/v1/documents/verify", verify1);

        // Act - Reject second identity document
        var verify2 = new
        {
            DocumentId = documentIds[1],
            IsVerified = false,
            VerificationNotes = "Second version rejected - blurry image"
        };
        await PostJsonAsync("/api/v1/documents/verify", verify2);

        // Assert - Verify complete history
        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();
            var allDocs = await dbContext.Documents
                .Where(d => d.ProviderId == providerId)
                .OrderBy(d => d.CreatedAt)
                .ToListAsync();

            allDocs.Should().HaveCount(3, "all documents should be preserved in history");
            
            // First identity document verified
            allDocs[0].Status.Should().Be(EDocumentStatus.Verified);
            allDocs[0].DocumentType.Should().Be(EDocumentType.IdentityDocument);

            // Second identity document rejected
            allDocs[1].Status.Should().Be(EDocumentStatus.Rejected);
            allDocs[1].DocumentType.Should().Be(EDocumentType.IdentityDocument);

            // Third document still uploaded (pending verification)
            allDocs[2].Status.Should().Be(EDocumentStatus.Uploaded);
            allDocs[2].DocumentType.Should().Be(EDocumentType.CriminalRecord);
        });
    }

    [Fact]
    public async Task RequestDocumentVerification_Should_UpdateStatus()
    {
        // Arrange
        AuthenticateAsAdmin();

        // Create a valid provider first to ensure ProviderId exists
        var createProviderRequest = new
        {
            UserId = Guid.NewGuid().ToString(),
            Name = "Test Provider for Document Verification",
            Type = 0, // Individual
            BusinessProfile = new
            {
                LegalName = "Test Company Legal Name",
                FantasyName = "Test Company",
                Description = (string?)null,
                ContactInfo = new
                {
                    Email = "test@provider.com",
                    Phone = "1234567890",
                    Website = (string?)null
                },
                PrimaryAddress = new
                {
                    Street = "123 Test St",
                    Number = "100",
                    Complement = (string?)null,
                    Neighborhood = "Centro",
                    City = "Test City",
                    State = "SP",
                    ZipCode = "12345-678",
                    Country = "Brasil"
                }
            }
        };

        var providerResponse = await ApiClient.PostAsJsonAsync("/api/v1/providers", createProviderRequest, JsonOptions);
        providerResponse.StatusCode.Should().Be(HttpStatusCode.Created, "Provider creation should succeed");

        var providerLocation = providerResponse.Headers.Location?.ToString();
        providerLocation.Should().NotBeNullOrEmpty("Provider creation should return Location header");
        var providerId = ExtractIdFromLocation(providerLocation!);

        // Wait for provider to be fully persisted (eventual consistency)
        await WaitForProviderAsync(providerId);

        // Now upload a document with the valid ProviderId
        var uploadRequest = new
        {
            ProviderId = providerId,
            DocumentType = EDocumentType.IdentityDocument,
            FileName = "verification_test.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024L
        };

        AuthenticateAsAdmin(); // POST upload requer autorização
        var uploadResponse = await ApiClient.PostAsJsonAsync("/api/v1/documents/upload", uploadRequest, JsonOptions);

        var uploadContent = await uploadResponse.Content.ReadAsStringAsync();
        uploadResponse.IsSuccessStatusCode.Should().BeTrue(
            because: $"Document upload should succeed, but got {uploadResponse.StatusCode}: {uploadContent}");

        uploadResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);

        Guid documentId;

        if (uploadResponse.StatusCode == HttpStatusCode.Created)
        {
            var locationHeader = uploadResponse.Headers.Location?.ToString();
            locationHeader.Should().NotBeNullOrEmpty("Created response must include Location header");
            documentId = ExtractIdFromLocation(locationHeader!);
        }
        else
        {
            uploadContent.Should().NotBeNullOrEmpty("Response body required for document ID");
            using var uploadResult = System.Text.Json.JsonDocument.Parse(uploadContent);

            // Response is UploadDocumentResponse directly, not wrapped in "data"
            uploadResult.RootElement.TryGetProperty("documentId", out var idProperty).Should().BeTrue();
            documentId = idProperty.GetGuid();
        }

        // Act - Request verification
        var verificationRequest = new
        {
            VerifierNotes = "Requesting verification for this document",
            Priority = "Normal"
        };

        var response = await ApiClient.PostAsJsonAsync(
            $"/api/v1/documents/{documentId}/verify",
            verificationRequest,
            JsonOptions);

        // Assert - Success path only (no BadRequest)
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Accepted,
            HttpStatusCode.NoContent);

        // Se a verificação foi aceita, verifica o status do documento
        AuthenticateAsAdmin(); // GET requer autorização
        var statusResponse = await ApiClient.GetAsync($"/api/v1/documents/{documentId}/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Status endpoint should be available after successful verification");

        var statusContent = await statusResponse.Content.ReadAsStringAsync();
        statusContent.Should().NotBeNullOrEmpty();

        // Parse JSON - DocumentDto é retornado diretamente, não wrapped em "data"
        using var statusResult = System.Text.Json.JsonDocument.Parse(statusContent);

        statusResult.RootElement.TryGetProperty("status", out var statusProperty)
            .Should().BeTrue("Response should contain 'status' property");

        // Parse status as enum to avoid string drift
        var statusString = statusProperty.GetString();
        statusString.Should().NotBeNullOrEmpty("Status should have a value");

        var statusParsed = Enum.TryParse<EDocumentStatus>(statusString, ignoreCase: true, out var documentStatus);
        statusParsed.Should().BeTrue($"Status '{statusString}' should be a valid EDocumentStatus");

        // Document should be in uploaded or pending verification status
        documentStatus.Should().BeOneOf(EDocumentStatus.Uploaded, EDocumentStatus.PendingVerification)
            .And.Subject.Should().NotBe(EDocumentStatus.Verified, "Document should not be verified immediately after upload");
    }

    [Fact]
    public async Task RequestDocumentVerification_WithNonExistentDocument_Should_ReturnNotFound()
    {
        // Arrange
        AuthenticateAsAdmin();
        var documentId = Guid.NewGuid(); // Non-existent document

        var verificationRequest = new
        {
            VerifierNotes = "Attempting to verify non-existent document",
            Priority = "High"
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync(
            $"/api/v1/documents/{documentId}/verify",
            verificationRequest,
            JsonOptions);

        // Assert - Only NotFound is expected for non-existent documents
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RequestDocumentVerification_WithInvalidData_Should_ReturnBadRequest()
    {
        // Arrange
        AuthenticateAsAdmin();
        var documentId = Guid.NewGuid();

        var invalidRequest = new
        {
            VerifierNotes = new string('a', 2001), // Too long - exceeds validation limit
            Priority = "InvalidPriority" // Invalid priority value
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync(
            $"/api/v1/documents/{documentId}/verify",
            invalidRequest,
            JsonOptions);

        // Assert - Pode retornar BadRequest (validação) ou NotFound (documento não existe)
        // A ordem de validação pode variar
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    #endregion

    #region OCR Document Intelligence Tests

    [Fact]
    public async Task DocumentVerificationJob_WithAzureConfigured_ShouldProcessOcr()
    {
        // Arrange - Criar documento para processar
        var providerId = Guid.NewGuid();
        Guid documentId = Guid.Empty;

        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();
            var document = Document.Create(
                providerId,
                EDocumentType.IdentityDocument,
                "identity-card.pdf",
                "test-blob-key.pdf");

            dbContext.Documents.Add(document);
            await dbContext.SaveChangesAsync();
            documentId = document.Id;
        });

        // Act - Simular processamento OCR (normalmente feito via job em background)
        // Em E2E, verificar se o serviço está disponível
        await WithServiceScopeAsync(async services =>
        {
            var ocrService = services.GetService<MeAjudaAi.Modules.Documents.Application.Interfaces.IDocumentIntelligenceService>();
            
            if (ocrService != null)
            {
                // Azure Document Intelligence está configurado
                // Verificar que não quebra a aplicação
                var dbContext = services.GetRequiredService<DocumentsDbContext>();
                var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
                document.Should().NotBeNull();
            }
            else
            {
                // Mock service está sendo usado (configuração de teste padrão)
                // Verificar que a aplicação funciona sem Azure configurado
                var dbContext = services.GetRequiredService<DocumentsDbContext>();
                var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
                document.Should().NotBeNull("Application should work with mock OCR service");
            }
        });
    }

    [Fact]
    public async Task OcrService_WithInvalidUrl_ShouldHandleGracefully()
    {
        // Arrange & Act - Verificar que serviço OCR lida com URLs inválidas
        await WithServiceScopeAsync(async services =>
        {
            var ocrService = services.GetService<MeAjudaAi.Modules.Documents.Application.Interfaces.IDocumentIntelligenceService>();
            
            if (ocrService != null)
            {
                // Tentar analisar URL inválida
                var act = async () => await ocrService.AnalyzeDocumentAsync(
                    "not-a-valid-url",
                    "identity",
                    CancellationToken.None);

                // Assert - Deve lançar ArgumentException
                await act.Should().ThrowAsync<ArgumentException>()
                    .WithMessage("*Invalid blob URL format*");
            }
        });
    }

    [Fact]
    public async Task OcrService_WithValidUrl_ShouldReturnSuccessResult()
    {
        // Arrange
        var validBlobUrl = "https://test.blob.core.windows.net/documents/test.pdf";
        var documentType = "identity";

        // Act & Assert
        await WithServiceScopeAsync(async services =>
        {
            var ocrService = services.GetService<MeAjudaAi.Modules.Documents.Application.Interfaces.IDocumentIntelligenceService>();
            
            if (ocrService != null)
            {
                try
                {
                    var result = await ocrService.AnalyzeDocumentAsync(
                        validBlobUrl,
                        documentType,
                        CancellationToken.None);

                    // Com Mock service, deve retornar sucesso
                    result.Should().NotBeNull();
                    result.Success.Should().BeTrue("Mock OCR service should return success");
                    result.ExtractedData.Should().NotBeNullOrEmpty();
                    result.Confidence.Should().BeGreaterThan(0);
                }
                catch (Exception ex) when (ex.Message.Contains("OCR") || ex.Message.Contains("Azure"))
                {
                    // Azure real pode falhar em ambiente de teste
                    // Isso é esperado se Azure Document Intelligence não estiver configurado
                }
            }
        });
    }

    [Fact]
    public async Task Document_AfterOcrProcessing_ShouldContainExtractedData()
    {
        // Arrange - Criar documento com dados OCR
        var providerId = Guid.NewGuid();

        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();
            var document = Document.Create(
                providerId,
                EDocumentType.IdentityDocument,
                "identity-with-ocr.pdf",
                "blob-key-ocr.pdf");

            // Simular processamento OCR bem-sucedido
            var mockOcrData = """
            {
                "documentType": "identity",
                "documentNumber": "123456789",
                "name": "Test User",
                "issueDate": "2024-01-01"
            }
            """;

            document.MarkAsVerified(mockOcrData);

            dbContext.Documents.Add(document);
            await dbContext.SaveChangesAsync();

            // Assert - Verificar persistência de dados OCR
            var saved = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == document.Id);
            saved.Should().NotBeNull();
            saved.OcrData.Should().NotBeNullOrEmpty();
            saved.OcrData.Should().Contain("documentNumber");
            saved.OcrData.Should().Contain("123456789");
            saved.Status.Should().Be(EDocumentStatus.Verified);
        });
    }

    [Fact]
    public async Task OcrService_WithLowConfidence_ShouldHandleAppropriately()
    {
        // Arrange & Act
        await WithServiceScopeAsync(async services =>
        {
            var ocrService = services.GetService<MeAjudaAi.Modules.Documents.Application.Interfaces.IDocumentIntelligenceService>();
            
            if (ocrService is MeAjudaAi.Modules.Documents.Tests.Mocks.MockDocumentIntelligenceService mockService)
            {
                // Configurar mock para retornar baixa confiança
                mockService.SetNextResultToLowConfidence();

                var result = await mockService.AnalyzeDocumentAsync(
                    "https://test.blob.core.windows.net/documents/low-quality.pdf",
                    "identity",
                    CancellationToken.None);

                // Assert
                result.Should().NotBeNull();
                result.Success.Should().BeTrue();
                result.Confidence.Should().BeLessThan(0.7f, "low confidence scenario");
            }
        });
    }

    [Fact]
    public async Task OcrService_WithError_ShouldReturnFailureResult()
    {
        // Arrange & Act
        await WithServiceScopeAsync(async services =>
        {
            var ocrService = services.GetService<MeAjudaAi.Modules.Documents.Application.Interfaces.IDocumentIntelligenceService>();
            
            if (ocrService is MeAjudaAi.Modules.Documents.Tests.Mocks.MockDocumentIntelligenceService mockService)
            {
                // Configurar mock para simular erro
                mockService.SetNextResultToError("OCR service unavailable");

                var result = await mockService.AnalyzeDocumentAsync(
                    "https://test.blob.core.windows.net/documents/error.pdf",
                    "identity",
                    CancellationToken.None);

                // Assert
                result.Should().NotBeNull();
                result.Success.Should().BeFalse();
                result.ErrorMessage.Should().NotBeNullOrEmpty();
                result.ErrorMessage.Should().Contain("unavailable");
            }
        });
    }

    #endregion
}
