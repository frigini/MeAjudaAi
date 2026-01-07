using Bunit;
using FluentAssertions;
using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Web.Admin.Features.Dashboard;
using MeAjudaAi.Web.Admin.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using static MeAjudaAi.Web.Admin.Features.Dashboard.DashboardActions;

namespace MeAjudaAi.Web.Admin.Tests.Pages;

/// <summary>
/// Testes para a página Dashboard.razor usando bUnit
/// </summary>
public class DashboardPageTests : Bunit.TestContext
{
    private readonly Mock<IProvidersApi> _mockProvidersApi;
    private readonly Mock<IServiceCatalogsApi> _mockServiceCatalogsApi;
    private readonly Mock<IDispatcher> _mockDispatcher;
    private readonly Mock<IState<DashboardState>> _mockDashboardState;

    public DashboardPageTests()
    {
        _mockProvidersApi = new Mock<IProvidersApi>();
        _mockServiceCatalogsApi = new Mock<IServiceCatalogsApi>();
        _mockDispatcher = new Mock<IDispatcher>();
        _mockDashboardState = new Mock<IState<DashboardState>>();

        // Configurar estado inicial vazio
        _mockDashboardState.Setup(x => x.Value).Returns(new DashboardState());

        // Registrar serviços necessários
        Services.AddSingleton(_mockProvidersApi.Object);
        Services.AddSingleton(_mockServiceCatalogsApi.Object);
        Services.AddSingleton(_mockDispatcher.Object);
        Services.AddSingleton(_mockDashboardState.Object);
        Services.AddMudServices();
        
        // Configurar JSInterop mock para MudBlazor
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Dashboard_Should_Dispatch_LoadDashboardStatsAction_OnInitialized()
    {
        // Arrange & Act
        var cut = RenderComponent<Dashboard>();

        // Assert
        _mockDispatcher.Verify(
            x => x.Dispatch(It.IsAny<LoadDashboardStatsAction>()), 
            Times.Once,
            "LoadDashboardStatsAction deve ser disparada ao inicializar o dashboard");
    }

    [Fact]
    public void Dashboard_Should_Display_Loading_State_When_IsLoading()
    {
        // Arrange
        _mockDashboardState.Setup(x => x.Value).Returns(new DashboardState
        {
            IsLoading = true
        });

        // Act
        var cut = RenderComponent<Dashboard>();

        // Assert
        var progressElements = cut.FindAll(".mud-progress-circular");
        progressElements.Should().NotBeEmpty("Deve exibir indicador de carregamento quando IsLoading = true");
    }

    [Fact]
    public void Dashboard_Should_Display_KPI_Values_When_Loaded()
    {
        // Arrange
        _mockDashboardState.Setup(x => x.Value).Returns(new DashboardState
        {
            IsLoading = false,
            TotalProviders = 42,
            PendingVerifications = 7,
            ActiveServices = 12
        });

        // Act
        var cut = RenderComponent<Dashboard>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("42", "Deve exibir o total de providers");
        markup.Should().Contain("7", "Deve exibir verificações pendentes");
        markup.Should().Contain("12", "Deve exibir serviços ativos");
    }

    [Fact]
    public void Dashboard_Should_Display_Error_Message_When_ErrorMessage_IsSet()
    {
        // Arrange
        var errorMessage = "Erro ao carregar estatísticas";
        _mockDashboardState.Setup(x => x.Value).Returns(new DashboardState
        {
            IsLoading = false,
            ErrorMessage = errorMessage
        });

        // Act
        var cut = RenderComponent<Dashboard>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain(errorMessage, "Deve exibir mensagem de erro quando ErrorMessage está definida");
    }
}
