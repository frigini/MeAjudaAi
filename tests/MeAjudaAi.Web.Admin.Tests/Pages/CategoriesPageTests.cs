using Bunit;
using FluentAssertions;
using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;
using MeAjudaAi.Web.Admin.Features.Modules.ServiceCatalogs;
using MeAjudaAi.Web.Admin.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using static MeAjudaAi.Web.Admin.Features.Modules.ServiceCatalogs.ServiceCatalogsActions;
using MeAjudaAi.Web.Admin.Tests.Helpers;

namespace MeAjudaAi.Web.Admin.Tests.Pages;

/// <summary>
/// Testes para a página Categories.razor usando bUnit
/// </summary>
public class CategoriesPageTests
{
    [Fact]
    public async Task Categories_Page_Should_Dispatch_LoadCategoriesAction_OnInitialized()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockServiceCatalogsApi = new Mock<IServiceCatalogsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockState = new Mock<IState<ServiceCatalogsState>>();
        
        mockState.Setup(x => x.Value).Returns(new ServiceCatalogsState());
        
        ctx.Services.AddSingleton(mockServiceCatalogsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockState.Object);

        // Act
        var cut = ctx.Render<Categories>();

        // Assert
        mockDispatcher.Verify(
            x => x.Dispatch(It.IsAny<LoadCategoriesAction>()), 
            Times.Once,
            "LoadCategoriesAction deve ser disparada ao inicializar a página");
    }

    [Fact]
    public async Task Categories_Page_Should_Show_Create_Button()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockServiceCatalogsApi = new Mock<IServiceCatalogsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockState = new Mock<IState<ServiceCatalogsState>>();
        
        mockState.Setup(x => x.Value).Returns(new ServiceCatalogsState());
        
        ctx.Services.AddSingleton(mockServiceCatalogsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockState.Object);

        // Act
        var cut = ctx.Render<Categories>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("Nova Categoria", "Deve ter botão de criar nova categoria");
    }

    [Fact]
    public async Task Categories_Page_Should_Display_Categories_List()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockServiceCatalogsApi = new Mock<IServiceCatalogsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockState = new Mock<IState<ServiceCatalogsState>>();
        
        var testCategory = new ModuleServiceCategoryDto(
            Guid.NewGuid(),
            "Limpeza",
            "Serviços de limpeza residencial",
            true,
            1
        );
        
        mockState.Setup(x => x.Value).Returns(new ServiceCatalogsState 
        { 
            Categories = new List<ModuleServiceCategoryDto> { testCategory }
        });
        
        ctx.Services.AddSingleton(mockServiceCatalogsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockState.Object);

        // Act
        var cut = ctx.Render<Categories>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("Limpeza", "Deve exibir nome da categoria");
    }

    [Fact]
    public async Task Categories_Page_Should_Show_Loading_Indicator_When_Loading()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockServiceCatalogsApi = new Mock<IServiceCatalogsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockState = new Mock<IState<ServiceCatalogsState>>();
        
        mockState.Setup(x => x.Value).Returns(new ServiceCatalogsState { IsLoadingCategories = true });
        
        ctx.Services.AddSingleton(mockServiceCatalogsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockState.Object);

        // Act
        var cut = ctx.Render<Categories>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("mud-progress-linear", "Indicador de loading deve estar visível.");
    }
}
