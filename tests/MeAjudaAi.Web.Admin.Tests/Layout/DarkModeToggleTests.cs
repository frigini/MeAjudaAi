using Bunit;
using FluentAssertions;
using Fluxor;
using MeAjudaAi.Web.Admin.Features.Theme;
using MeAjudaAi.Web.Admin.Layout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using static MeAjudaAi.Web.Admin.Features.Theme.ThemeActions;

namespace MeAjudaAi.Web.Admin.Tests.Layout;

/// <summary>
/// Testes para o dark mode toggle no MainLayout.razor usando bUnit
/// </summary>
public class DarkModeToggleTests : Bunit.BunitContext
{
    private readonly Mock<IDispatcher> _mockDispatcher;
    private readonly Mock<IState<ThemeState>> _mockThemeState;

    public DarkModeToggleTests()
    {
        _mockDispatcher = new Mock<IDispatcher>();
        _mockThemeState = new Mock<IState<ThemeState>>();

        // Configurar estado inicial (light mode)
        _mockThemeState.Setup(x => x.Value).Returns(new ThemeState { IsDarkMode = false });

        // Registrar serviços necessários
        Services.AddSingleton(_mockDispatcher.Object);
        Services.AddSingleton(_mockThemeState.Object);
        Services.AddMudServices();
        
        // Configurar JSInterop mock para MudBlazor
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void MainLayout_Should_Dispatch_ToggleDarkModeAction_When_Button_Clicked()
    {
        // Arrange
        var cut = Render<MainLayout>();

        // Encontrar botões que podem ser o dark mode toggle
        var buttons = cut.FindAll("button");
        buttons.Should().NotBeEmpty("Deve haver botões no layout");

        // Act
        // Simular clique no botão de dark mode (segundo botão no AppBar)
        if (buttons.Count >= 2)
        {
            buttons[1].Click();

            // Assert
            _mockDispatcher.Verify(
                x => x.Dispatch(It.IsAny<ToggleDarkModeAction>()), 
                Times.AtLeastOnce,
                "ToggleDarkModeAction deve ser disparada ao clicar no botão");
        }
    }

    [Fact]
    public void ThemeState_Should_Initialize_With_LightMode_By_Default()
    {
        // Arrange & Act
        var initialState = new ThemeState();

        // Assert
        initialState.IsDarkMode.Should().BeFalse("Estado inicial deve ser light mode");
    }
}
