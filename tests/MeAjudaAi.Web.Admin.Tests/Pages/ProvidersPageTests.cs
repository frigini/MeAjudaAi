using Bunit;
using FluentAssertions;
using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Web.Admin.Features.Providers;
using MeAjudaAi.Web.Admin.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using static MeAjudaAi.Web.Admin.Features.Providers.ProvidersActions;

namespace MeAjudaAi.Web.Admin.Tests.Pages;

/// <summary>
/// Testes para a página Providers.razor usando bUnit
/// </summary>
public class ProvidersPageTests : Bunit.TestContext
{
    private readonly Mock<IProvidersApi> _mockProvidersApi;
    private readonly Mock<IDispatcher> _mockDispatcher;
    private readonly Mock<IState<ProvidersState>> _mockProvidersState;

    public ProvidersPageTests()
    {
        _mockProvidersApi = new Mock<IProvidersApi>();
        _mockDispatcher = new Mock<IDispatcher>();
        _mockProvidersState = new Mock<IState<ProvidersState>>();

        // Configurar estado inicial vazio
        _mockProvidersState.Setup(x => x.Value).Returns(new ProvidersState());

        // Registrar serviços necessários
        Services.AddSingleton(_mockProvidersApi.Object);
        Services.AddSingleton(_mockDispatcher.Object);
        Services.AddSingleton(_mockProvidersState.Object);
        Services.AddMudServices();
        
        // Configurar JSInterop mock para MudBlazor
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Providers_Page_Should_Dispatch_LoadProvidersAction_OnInitialized()
    {
        // Arrange & Act
        var cut = RenderComponent<Providers>();

        // Assert
        _mockDispatcher.Verify(
            x => x.Dispatch(It.IsAny<LoadProvidersAction>()), 
            Times.Once,
            "LoadProvidersAction deve ser disparada ao inicializar a página");
    }

    [Fact]
    public void Providers_Page_Should_Show_Loading_Indicator_When_Loading()
    {
        // Arrange
        _mockProvidersState.Setup(x => x.Value).Returns(new ProvidersState { IsLoading = true });

        // Act
        var cut = RenderComponent<Providers>();

        // Assert
        var progressBars = cut.FindAll(".mud-progress-linear");
        progressBars.Should().NotBeEmpty("Indicador de loading deve estar visível quando IsLoading=true");
    }

    [Fact]
    public void Providers_Page_Should_Show_Error_Message_When_Error_Exists()
    {
        // Arrange
        const string errorMessage = "Erro ao carregar fornecedores";
        _mockProvidersState.Setup(x => x.Value).Returns(new ProvidersState 
        { 
            ErrorMessage = errorMessage 
        });

        // Act
        var cut = RenderComponent<Providers>();

        // Assert
        var alerts = cut.FindAll(".mud-alert");
        alerts.Should().NotBeEmpty();
        cut.Markup.Should().Contain(errorMessage, "Mensagem de erro deve ser exibida");
    }

    [Fact]
    public void Providers_Page_Should_Display_Providers_In_DataGrid()
    {
        // Arrange
        var providers = new List<ModuleProviderDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Fornecedor Teste",
                Email = "teste@exemplo.com",
                Document = "12345678901",
                Phone = "11999999999",
                ProviderType = "Individual",
                VerificationStatus = "Verified",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        _mockProvidersState.Setup(x => x.Value).Returns(new ProvidersState 
        { 
            Providers = providers
        });

        // Act
        var cut = RenderComponent<Providers>();

        // Assert
        cut.Markup.Should().Contain("Fornecedor Teste", "Provider deve estar renderizado");
        cut.Markup.Should().Contain("teste@exemplo.com");
    }
}
