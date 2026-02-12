using Bunit;
using FluentAssertions;
using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Web.Admin.Features.Modules.Locations;
using MeAjudaAi.Web.Admin.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using static MeAjudaAi.Web.Admin.Features.Modules.Locations.LocationsActions;
using MeAjudaAi.Web.Admin.Tests.Helpers;

namespace MeAjudaAi.Web.Admin.Tests.Pages;

/// <summary>
/// Testes para a página AllowedCities.razor usando bUnit
/// </summary>
public class AllowedCitiesPageTests
{
    [Fact]
    public async Task AllowedCities_Page_Should_Dispatch_LoadAction_OnInitialized()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockLocationsApi = new Mock<ILocationsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockState = new Mock<IState<LocationsState>>();
        
        mockState.Setup(x => x.Value).Returns(new LocationsState());
        
        ctx.Services.AddSingleton(mockLocationsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockState.Object);

        // Act
        var cut = ctx.Render<AllowedCities>();

        // Assert
        mockDispatcher.Verify(
            x => x.Dispatch(It.IsAny<LoadAllowedCitiesAction>()), 
            Times.Once,
            "LoadAllowedCitiesAction deve ser disparada ao inicializar");
    }

    [Fact]
    public async Task AllowedCities_Page_Should_Show_Create_Button()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockLocationsApi = new Mock<ILocationsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockState = new Mock<IState<LocationsState>>();
        
        mockState.Setup(x => x.Value).Returns(new LocationsState());
        
        ctx.Services.AddSingleton(mockLocationsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockState.Object);

        // Act
        var cut = ctx.Render<AllowedCities>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("Nova Cidade", "Deve ter botão de criar nova cidade");
    }

    [Fact]
    public async Task AllowedCities_Page_Should_Display_Cities_List()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockLocationsApi = new Mock<ILocationsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockState = new Mock<IState<LocationsState>>();
        
        var testCity = new ModuleAllowedCityDto(
            Guid.NewGuid(),
            "São Paulo",
            "SP",
            "Brasil",
            -23.5505,
            -46.6333,
            50,
            true,
            DateTime.UtcNow,
            null
        );
        
        mockState.Setup(x => x.Value).Returns(new LocationsState 
        { 
            AllowedCities = new List<ModuleAllowedCityDto> { testCity }
        });
        
        ctx.Services.AddSingleton(mockLocationsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockState.Object);

        // Act
        var cut = ctx.Render<AllowedCities>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("São Paulo", "Deve exibir nome da cidade");
        markup.Should().Contain("SP", "Deve exibir estado");
    }

    [Fact]
    public async Task AllowedCities_Page_Should_Show_Loading_Indicator()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockLocationsApi = new Mock<ILocationsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockState = new Mock<IState<LocationsState>>();
        
        mockState.Setup(x => x.Value).Returns(new LocationsState { IsLoading = true });
        
        ctx.Services.AddSingleton(mockLocationsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockState.Object);

        // Act
        var cut = ctx.Render<AllowedCities>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("mud-progress-linear", "Indicador de loading deve estar visível.");
    }
}
