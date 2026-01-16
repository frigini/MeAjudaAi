using Bunit;
using FluentAssertions;
using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;
using MeAjudaAi.Web.Admin.Features.ServiceCatalogs;
using MeAjudaAi.Web.Admin.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using static MeAjudaAi.Web.Admin.Features.ServiceCatalogs.ServiceCatalogsActions;

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
        using var ctx = new Bunit.TestContext();
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
        var cut = ctx.RenderComponent<Services>();

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
        using var ctx = new Bunit.TestContext();
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
        var cut = ctx.RenderComponent<Services>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("Novo Serviço", "Deve ter botão de criar novo serviço");
    }

    [Fact]
    public void Services_Page_Should_Display_Services_List()
    {
        // Arrange
        using var ctx = new Bunit.TestContext();
        var mockServiceCatalogsApi = new Mock<IServiceCatalogsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockState = new Mock<IState<ServiceCatalogsState>>();
        
        var categoryId = Guid.NewGuid();
        var testService = new ModuleServiceListDto(
            Guid.NewGuid(),
            categoryId,
            "Limpeza Residencial",
            "Limpeza completa de residências",
            1,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        
        var testCategory = new ModuleServiceCategoryDto(
            categoryId,
            "Limpeza",
            "Categoria de limpeza",
            1,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow
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
        var cut = ctx.RenderComponent<Services>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("Limpeza Residencial", "Deve exibir nome do serviço");
    }
}
