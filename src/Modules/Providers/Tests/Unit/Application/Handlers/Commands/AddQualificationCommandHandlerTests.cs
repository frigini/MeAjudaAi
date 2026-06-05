using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class AddQualificationCommandHandlerTests
{
    private readonly Mock<IProviderUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<ILogger<AddQualificationCommandHandler>> _loggerMock;
    private readonly Mock<IStringLocalizer<Strings>> _localizerMock;
    private readonly AddQualificationCommandHandler _handler;

    public AddQualificationCommandHandlerTests()
    {
        _uowMock = new Mock<IProviderUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _loggerMock = new Mock<ILogger<AddQualificationCommandHandler>>();
        _localizerMock = new Mock<IStringLocalizer<Strings>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        
        // Setup localizer to return specific Portuguese messages
        _localizerMock.Setup(l => l["ProviderNotFound"]).Returns(new LocalizedString("ProviderNotFound", "Prestador não encontrado."));
        _localizerMock.Setup(l => l["QualificationAddError"]).Returns(new LocalizedString("QualificationAddError", "Ocorreu um erro ao adicionar a qualificação."));

        _handler = new AddQualificationCommandHandler(_uowMock.Object, _loggerMock.Object, _localizerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId).Build();
        var command = new AddQualificationCommand(
            ProviderId: providerId,
            Name: "Certificação AWS",
            Description: "AWS Solutions Architect Associate",
            IssuingOrganization: "Amazon Web Services",
            IssueDate: new DateTime(2023, 1, 1),
            ExpirationDate: new DateTime(2025, 1, 1),
            DocumentNumber: "AWS-SAA-123456"
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Qualifications.Should().ContainSingle(q => q.Name == command.Name);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new AddQualificationCommand(
            ProviderId: providerId,
            Name: "Certificação AWS",
            Description: "AWS Solutions Architect Associate",
            IssuingOrganization: "Amazon Web Services",
            IssueDate: new DateTime(2023, 1, 1),
            ExpirationDate: new DateTime(2025, 1, 1),
            DocumentNumber: null
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("Prestador não encontrado.");
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new AddQualificationCommand(
            ProviderId: providerId,
            Name: "Certificação AWS",
            Description: "AWS Solutions Architect Associate",
            IssuingOrganization: "Amazon Web Services",
            IssueDate: new DateTime(2023, 1, 1),
            ExpirationDate: new DateTime(2025, 1, 1),
            DocumentNumber: null
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("Ocorreu um erro ao adicionar a qualificação.");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId).Build();
        var command = new AddQualificationCommand(
            ProviderId: providerId,
            Name: "",
            Description: "Description",
            IssuingOrganization: "Organization",
            IssueDate: new DateTime(2023, 1, 1),
            ExpirationDate: new DateTime(2025, 1, 1),
            DocumentNumber: null
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("Ocorreu um erro ao adicionar a qualificação.");
    }

    [Fact]
    public async Task HandleAsync_WithIssueDateAfterExpirationDate_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId).Build();
        var command = new AddQualificationCommand(
            ProviderId: providerId,
            Name: "Certificação AWS",
            Description: "AWS Solutions Architect Associate",
            IssuingOrganization: "Amazon Web Services",
            IssueDate: new DateTime(2025, 1, 1),
            ExpirationDate: new DateTime(2023, 1, 1),
            DocumentNumber: null
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("Ocorreu um erro ao adicionar a qualificação.");
    }
}


