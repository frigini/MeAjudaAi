using Bunit;
using FluentAssertions;
using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Web.Admin.Features.Modules.Providers;
using MeAjudaAi.Web.Admin.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using static MeAjudaAi.Web.Admin.Features.Modules.Providers.ProvidersActions;
using MeAjudaAi.Web.Admin.Services.Interfaces;

namespace MeAjudaAi.Web.Admin.Tests.Pages;

/// <summary>
/// Testes para a página Providers.razor usando bUnit
/// </summary>
public class ProvidersPageTests
{
    [Fact]
    public void Providers_Page_Should_Dispatch_LoadProvidersAction_OnInitialized()
    {
        // Arrange
        using var ctx = new Bunit.BunitContext();
        var mockProvidersApi = new Mock<IProvidersApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockProvidersState = new Mock<IState<ProvidersState>>();
        var mockPermissionService = new Mock<IPermissionService>();
        var mockActionSubscriber = new Mock<IActionSubscriber>();
        
        mockProvidersState.Setup(x => x.Value).Returns(new ProvidersState());
        mockPermissionService.Setup(x => x.HasPermissionAsync(It.IsAny<string>())).ReturnsAsync(true);
        
        ctx.Services.AddSingleton(mockProvidersApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockProvidersState.Object);
        ctx.Services.AddSingleton(mockPermissionService.Object);
        ctx.Services.AddSingleton(mockActionSubscriber.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.Render<Providers>();

        // Assert
        mockDispatcher.Verify(
            x => x.Dispatch(It.IsAny<LoadProvidersAction>()), 
            Times.Once,
            "LoadProvidersAction deve ser disparada ao inicializar a página");
    }

    [Fact]
    public void Providers_Page_Should_Show_Loading_Indicator_When_Loading()
    {
        // Arrange
        using var ctx = new Bunit.BunitContext();
        var mockProvidersApi = new Mock<IProvidersApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockProvidersState = new Mock<IState<ProvidersState>>();
        var mockPermissionService = new Mock<IPermissionService>();
        var mockActionSubscriber = new Mock<IActionSubscriber>();
        
        mockProvidersState.Setup(x => x.Value).Returns(new ProvidersState { IsLoading = true });
        mockPermissionService.Setup(x => x.HasPermissionAsync(It.IsAny<string>())).ReturnsAsync(true);
        
        ctx.Services.AddSingleton(mockProvidersApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockProvidersState.Object);
        ctx.Services.AddSingleton(mockPermissionService.Object);
        ctx.Services.AddSingleton(mockActionSubscriber.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.Render<Providers>();

        // Assert
        var progressBars = cut.FindAll(".mud-progress-linear");
        progressBars.Should().NotBeEmpty("Indicador de loading deve estar visível quando IsLoading=true");
    }

    [Fact]
    public void Providers_Page_Should_Show_Error_Message_When_Error_Exists()
    {
        // Arrange
        using var ctx = new Bunit.BunitContext();
        var mockProvidersApi = new Mock<IProvidersApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockProvidersState = new Mock<IState<ProvidersState>>();
        
        const string errorMessage = "Erro ao carregar fornecedores";
        mockProvidersState.Setup(x => x.Value).Returns(new ProvidersState 
        { 
            ErrorMessage = errorMessage 
        });
        
        var mockPermissionService = new Mock<IPermissionService>();
        var mockActionSubscriber = new Mock<IActionSubscriber>();
        mockPermissionService.Setup(x => x.HasPermissionAsync(It.IsAny<string>())).ReturnsAsync(true);
        
        ctx.Services.AddSingleton(mockProvidersApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockProvidersState.Object);
        ctx.Services.AddSingleton(mockPermissionService.Object);
        ctx.Services.AddSingleton(mockActionSubscriber.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.Render<Providers>();

        // Assert
        var alerts = cut.FindAll(".mud-alert");
        alerts.Should().NotBeEmpty();
        cut.Markup.Should().Contain(errorMessage, "Mensagem de erro deve ser exibida");
    }

    [Fact]
    public void Providers_Page_Should_Display_Providers_In_DataGrid()
    {
        // Arrange
        using var ctx = new Bunit.BunitContext();
        var mockProvidersApi = new Mock<IProvidersApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockProvidersState = new Mock<IState<ProvidersState>>();
        
        var providers = new List<ModuleProviderDto>
        {
            new(
                Id: Guid.NewGuid(),
                Name: "Fornecedor Teste",
                Email: "teste@exemplo.com",
                Document: "12345678901",
                ProviderType: "Individual",
                VerificationStatus: "Verified",
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: DateTime.UtcNow,
                IsActive: true,
                Phone: "11999999999"
            )
        };

        mockProvidersState.Setup(x => x.Value).Returns(new ProvidersState 
        { 
        });
        
        var mockPermissionService = new Mock<IPermissionService>();
        var mockActionSubscriber = new Mock<IActionSubscriber>();
        mockPermissionService.Setup(x => x.HasPermissionAsync(It.IsAny<string>())).ReturnsAsync(true);
        
        ctx.Services.AddSingleton(mockProvidersApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockProvidersState.Object);
        ctx.Services.AddSingleton(mockPermissionService.Object);
        ctx.Services.AddSingleton(mockActionSubscriber.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.Render<Providers>();

        // Assert
        cut.Markup.Should().Contain("Fornecedor Teste", "Provider deve estar renderizado");
        cut.Markup.Should().Contain("teste@exemplo.com");
    }
}
