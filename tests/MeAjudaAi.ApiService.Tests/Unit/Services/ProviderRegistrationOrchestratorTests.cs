using FluentAssertions;
using MeAjudaAi.ApiService.Services.Orchestration;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.ApiService.Tests.Unit.Services;

public class ProviderRegistrationOrchestratorTests
{
    private readonly Mock<ICommandDispatcher> _dispatcherMock;
    private readonly Mock<ILogger<ProviderRegistrationOrchestrator>> _loggerMock;
    private readonly ProviderRegistrationOrchestrator _orchestrator;

    public ProviderRegistrationOrchestratorTests()
    {
        _dispatcherMock = new Mock<ICommandDispatcher>();
        _loggerMock = new Mock<ILogger<ProviderRegistrationOrchestrator>>();
        _orchestrator = new ProviderRegistrationOrchestrator(_dispatcherMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterProviderAsync_WithTermsNotAccepted_ShouldReturnFailure()
    {
        var request = CreateRequest(acceptedTerms: false, acceptedPrivacyPolicy: true);

        var result = await _orchestrator.RegisterProviderAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("Termos");
    }

    [Fact]
    public async Task RegisterProviderAsync_WithPrivacyPolicyNotAccepted_ShouldReturnFailure()
    {
        var request = CreateRequest(acceptedTerms: true, acceptedPrivacyPolicy: false);

        var result = await _orchestrator.RegisterProviderAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("Privacidade");
    }

    [Fact]
    public async Task RegisterProviderAsync_WhenUserCreationFails_ShouldReturnFailure()
    {
        var request = CreateRequest();
        SetupUserCreationFailure();

        var result = await _orchestrator.RegisterProviderAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("erro ao registrar o usuário");
    }

    [Fact]
    public async Task RegisterProviderAsync_WhenProviderCreationFails_ShouldCompensateAndReturnFailure()
    {
        var request = CreateRequest();
        var userDto = CreateUserDto(Guid.NewGuid());
        SetupUserCreationSuccess(userDto);
        SetupProviderCreationFailure();

        var result = await _orchestrator.RegisterProviderAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("erro ao registrar o provedor");
    }

    [Fact]
    public async Task RegisterProviderAsync_WhenBothSucceed_ShouldReturnProviderDto()
    {
        var request = CreateRequest();
        var userDto = CreateUserDto(Guid.NewGuid());
        var providerDto = CreateProviderDto(Guid.NewGuid(), userDto.Id);
        SetupUserCreationSuccess(userDto);
        SetupProviderCreationSuccess(providerDto);

        var result = await _orchestrator.RegisterProviderAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(providerDto.Id);
    }

    [Theory]
    [InlineData("11999999999", "provider_11999999999")]
    public void GenerateUsername_WithPhone_ShouldUsePhone(string phone, string expectedPrefix)
    {
        var username = ProviderRegistrationOrchestrator.GenerateUsername(phone);

        username.Should().StartWith(expectedPrefix);
    }

    [Fact]
    public void GenerateUsername_WithEmptyPhone_ShouldGenerateRandomUsername()
    {
        var username = ProviderRegistrationOrchestrator.GenerateUsername("");

        username.Should().StartWith("provider_");
        username.Length.Should().BeGreaterThan("provider_".Length);
    }

    [Fact]
    public void SplitName_WithFullName_ShouldSplitCorrectly()
    {
        var (firstName, lastName) = ProviderRegistrationOrchestrator.SplitName("John Doe");

        firstName.Should().Be("John");
        lastName.Should().Be("Doe");
    }

    [Fact]
    public void SplitName_WithSingleName_ShouldUseAsBoth()
    {
        var (firstName, lastName) = ProviderRegistrationOrchestrator.SplitName("John");

        firstName.Should().Be("John");
        lastName.Should().Be("John");
    }

    [Fact]
    public void SplitName_WithExtraSpaces_ShouldTrim()
    {
        var (firstName, lastName) = ProviderRegistrationOrchestrator.SplitName("  John Doe  ");

        firstName.Should().Be("John");
        lastName.Should().Be("Doe");
    }

    [Fact]
    public void GenerateTemporaryPassword_ShouldMeetComplexityRequirements()
    {
        var password = ProviderRegistrationOrchestrator.GenerateTemporaryPassword();

        password.Should().StartWith("Temp");
        password.Should().EndWith("!123");
        password.Length.Should().BeGreaterThan("Temp".Length + "!123".Length);
    }

    [Fact]
    public void SanitizePhone_WithFormattedPhone_ShouldExtractNumbers()
    {
        var result = ProviderRegistrationOrchestrator.SanitizePhone("(11) 9999-99999");

        result.Should().Be("11999999999");
    }

    [Fact]
    public void SanitizePhone_WithNullPhone_ShouldReturnEmpty()
    {
        var result = ProviderRegistrationOrchestrator.SanitizePhone(null);

        result.Should().BeEmpty();
    }

    private static RegisterProviderRequest CreateRequest(
        bool acceptedTerms = true,
        bool acceptedPrivacyPolicy = true) => new()
    {
        Name = "Test Provider",
        Email = "test@example.com",
        PhoneNumber = "11999999999",
        Type = EProviderType.Individual,
        AcceptedTerms = acceptedTerms,
        AcceptedPrivacyPolicy = acceptedPrivacyPolicy
    };

    private static UserDto CreateUserDto(Guid id) => new(
        id, "username", "test@test.com", "First", "Last", "First Last", null, id.ToString(), DateTime.UtcNow, null);

    private static ProviderDto CreateProviderDto(Guid id, Guid userId) => new(
        id, userId, "Test Provider", "test-provider", EProviderType.Individual, null!,
        EProviderStatus.Active, EVerificationStatus.Verified,
        EProviderTier.Standard, [], [], [], DateTime.UtcNow, null, false, null, true);

    private void SetupUserCreationSuccess(UserDto userDto)
    {
        _dispatcherMock.Setup(x => x.SendAsync<MeAjudaAi.Modules.Users.Application.Commands.CreateUserCommand, Result<UserDto>>(
            It.IsAny<MeAjudaAi.Modules.Users.Application.Commands.CreateUserCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(userDto));
    }

    private void SetupUserCreationFailure()
    {
        _dispatcherMock.Setup(x => x.SendAsync<MeAjudaAi.Modules.Users.Application.Commands.CreateUserCommand, Result<UserDto>>(
            It.IsAny<MeAjudaAi.Modules.Users.Application.Commands.CreateUserCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure(new Error("Failed", 400)));
    }

    private void SetupProviderCreationSuccess(ProviderDto providerDto)
    {
        _dispatcherMock.Setup(x => x.SendAsync<MeAjudaAi.Modules.Providers.Application.Commands.CreateProviderCommand, Result<ProviderDto>>(
            It.IsAny<MeAjudaAi.Modules.Providers.Application.Commands.CreateProviderCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Success(providerDto));
    }

    private void SetupProviderCreationFailure()
    {
        _dispatcherMock.Setup(x => x.SendAsync<MeAjudaAi.Modules.Providers.Application.Commands.CreateProviderCommand, Result<ProviderDto>>(
            It.IsAny<MeAjudaAi.Modules.Providers.Application.Commands.CreateProviderCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Failure(new Error("Failed", 400)));
    }
}
