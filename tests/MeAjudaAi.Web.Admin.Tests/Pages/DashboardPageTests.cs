using Bunit;
using FluentAssertions;
using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Web.Admin.Features.Dashboard;
using MeAjudaAi.Web.Admin.Features.Modules.Providers;
using MeAjudaAi.Web.Admin.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using static MeAjudaAi.Web.Admin.Features.Dashboard.DashboardActions;
using MeAjudaAi.Web.Admin.Tests.Helpers;

namespace MeAjudaAi.Web.Admin.Tests.Pages;

/// <summary>
/// Testes para a página Dashboard.razor usando bUnit
/// </summary>
public class DashboardPageTests
{
    [Fact]
    public async Task Dashboard_Should_Dispatch_LoadDashboardStatsAction_OnInitialized()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockProvidersApi = new Mock<IProvidersApi>();
        var mockServiceCatalogsApi = new Mock<IServiceCatalogsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDashboardState = new Mock<IState<DashboardState>>();
        var mockProvidersState = new Mock<IState<ProvidersState>>();

        mockDashboardState.Setup(x => x.Value).Returns(new DashboardState());
        mockProvidersState.Setup(x => x.Value).Returns(new ProvidersState());

        ctx.Services.AddSingleton(mockProvidersApi.Object);
        ctx.Services.AddSingleton(mockServiceCatalogsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockDashboardState.Object);
        ctx.Services.AddSingleton(mockProvidersState.Object);

        // Act
        var cut = ctx.Render<Dashboard>();

        // Assert
        mockDispatcher.Verify(
            x => x.Dispatch(It.IsAny<LoadDashboardStatsAction>()), 
            Times.Once,
            "LoadDashboardStatsAction deve ser disparada ao inicializar o dashboard");
    }

    [Fact]
    public async Task Dashboard_Should_Display_Loading_State_When_IsLoading()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockProvidersApi = new Mock<IProvidersApi>();
        var mockServiceCatalogsApi = new Mock<IServiceCatalogsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDashboardState = new Mock<IState<DashboardState>>();
        var mockProvidersState = new Mock<IState<ProvidersState>>();

        mockDashboardState.Setup(x => x.Value).Returns(new DashboardState { IsLoading = true });
        mockProvidersState.Setup(x => x.Value).Returns(new ProvidersState());

        ctx.Services.AddSingleton(mockProvidersApi.Object);
        ctx.Services.AddSingleton(mockServiceCatalogsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockDashboardState.Object);
        ctx.Services.AddSingleton(mockProvidersState.Object);

        // Act
        var cut = ctx.Render<Dashboard>();

        // Assert
        var progressElements = cut.FindAll(".mud-progress-circular, .mud-progress-linear");
        progressElements.Should().NotBeEmpty("Deve exibir indicador de carregamento quando IsLoading = true.");
    }

    [Fact]
    public async Task Dashboard_Should_Display_KPI_Values_When_Loaded()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockProvidersApi = new Mock<IProvidersApi>();
        var mockServiceCatalogsApi = new Mock<IServiceCatalogsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDashboardState = new Mock<IState<DashboardState>>();
        var mockProvidersState = new Mock<IState<ProvidersState>>();

        mockDashboardState.Setup(x => x.Value).Returns(new DashboardState
        {
            IsLoading = false,
            TotalProviders = 42,
            PendingVerifications = 7,
            ActiveServices = 12
        });
        mockProvidersState.Setup(x => x.Value).Returns(new ProvidersState());

        ctx.Services.AddSingleton(mockProvidersApi.Object);
        ctx.Services.AddSingleton(mockServiceCatalogsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockDashboardState.Object);
        ctx.Services.AddSingleton(mockProvidersState.Object);

        // Act
        var cut = ctx.Render<Dashboard>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("42", "Deve exibir o total de providers");
        markup.Should().Contain("7", "Deve exibir verificações pendentes");
        markup.Should().Contain("12", "Deve exibir serviços ativos");
    }

    [Fact]
    public async Task Dashboard_Should_Display_Error_Message_When_ErrorMessage_IsSet()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockProvidersApi = new Mock<IProvidersApi>();
        var mockServiceCatalogsApi = new Mock<IServiceCatalogsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDashboardState = new Mock<IState<DashboardState>>();
        var mockProvidersState = new Mock<IState<ProvidersState>>();

        var errorMessage = "Erro ao carregar estatísticas";
        mockDashboardState.Setup(x => x.Value).Returns(new DashboardState
        {
            IsLoading = false,
            ErrorMessage = errorMessage
        });
        mockProvidersState.Setup(x => x.Value).Returns(new ProvidersState());

        ctx.Services.AddSingleton(mockProvidersApi.Object);
        ctx.Services.AddSingleton(mockServiceCatalogsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockDashboardState.Object);
        ctx.Services.AddSingleton(mockProvidersState.Object);

        // Act
        var cut = ctx.Render<Dashboard>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain(errorMessage, "Deve exibir mensagem de erro quando ErrorMessage está definida");
    }
}
