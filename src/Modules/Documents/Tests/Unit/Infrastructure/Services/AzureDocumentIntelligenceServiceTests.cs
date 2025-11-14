using Azure.AI.DocumentIntelligence;
using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Constants;
using MeAjudaAi.Modules.Documents.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Testes unitários para Azure DocumentIntelligenceService.
/// Nota: Devido à complexidade do Azure SDK (classes seladas, sem interfaces), estes testes focam em:
/// - Validação de parâmetros
/// - Lógica de mapeamento de document types
/// - Guard clauses
/// Para testes de integração real com Azure, veja DocumentsIntegrationTests.
/// </summary>
public class AzureDocumentIntelligenceServiceTests
{
    private readonly Mock<ILogger<AzureDocumentIntelligenceService>> _mockLogger;

    public AzureDocumentIntelligenceServiceTests()
    {
        _mockLogger = new Mock<ILogger<AzureDocumentIntelligenceService>>();
    }

    [Fact]
    public void Constructor_WhenClientIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new AzureDocumentIntelligenceService(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("client");
    }
}
