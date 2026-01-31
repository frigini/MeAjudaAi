using Bunit;
using FluentAssertions;
using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;
using MeAjudaAi.Web.Admin.Features.Modules.ServiceCatalogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using static MeAjudaAi.Web.Admin.Features.Modules.ServiceCatalogs.ServiceCatalogsActions;
using ServicesPage = MeAjudaAi.Web.Admin.Pages.Services;

namespace MeAjudaAi.Web.Admin.Tests.Pages;

/// <summary>
/// Testes para a página Services.razor usando bUnit
/// </summary>
public class ServicesPageTests
{
    [Fact]
    public void Services_Page_Should_Dispatch_LoadActions_OnInitialized()
    {
        // Arrange
        using var ctx = new Bunit.BunitContext();
        var mockServiceCatalogsApi = new Mock<IServiceCatalogsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockState = new Mock<IState<ServiceCatalogsState>>();
        
        mockState.Setup(x => x.Value).Returns(new ServiceCatalogsState());
        
        ctx.Services.AddSingleton(mockServiceCatalogsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockState.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.Render<ServicesPage>();

        // Assert
        mockDispatcher.Verify(
            x => x.Dispatch(It.IsAny<LoadServicesAction>()), 
            Times.Once,
            "LoadServicesAction deve ser disparada");
        mockDispatcher.Verify(
            x => x.Dispatch(It.IsAny<LoadCategoriesAction>()), 
            Times.Once,
            "LoadCategoriesAction deve ser disparada para popular dropdown");
    }

    [Fact]
    public void Services_Page_Should_Show_Create_Button()
    {
        // Arrange
        using var ctx = new Bunit.BunitContext();
        var mockServiceCatalogsApi = new Mock<IServiceCatalogsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockState = new Mock<IState<ServiceCatalogsState>>();
        
        mockState.Setup(x => x.Value).Returns(new ServiceCatalogsState());
        
        ctx.Services.AddSingleton(mockServiceCatalogsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockState.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.Render<ServicesPage>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("Novo Serviço", "Deve ter botão de criar novo serviço");
    }

    [Fact]
    public void Services_Page_Should_Display_Services_List()
    {
        // Arrange
        using var ctx = new Bunit.BunitContext();
        var mockServiceCatalogsApi = new Mock<IServiceCatalogsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockState = new Mock<IState<ServiceCatalogsState>>();
        
        var categoryId = Guid.NewGuid();
        var testService = new ModuleServiceListDto(
            Guid.NewGuid(),
            categoryId,
            "Limpeza Residencial",
            null,
            0,
            true
        );
        
        var testCategory = new ModuleServiceCategoryDto(
            categoryId,
            "Limpeza",
            "Categoria de limpeza",
            true,
            1
        );
        
        mockState.Setup(x => x.Value).Returns(new ServiceCatalogsState 
        { 
            Services = new List<ModuleServiceListDto> { testService },
            Categories = new List<ModuleServiceCategoryDto> { testCategory }
        });
        
        ctx.Services.AddSingleton(mockServiceCatalogsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockState.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.Render<ServicesPage>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("Limpeza Residencial", "Deve exibir nome do serviço");
    }
}
