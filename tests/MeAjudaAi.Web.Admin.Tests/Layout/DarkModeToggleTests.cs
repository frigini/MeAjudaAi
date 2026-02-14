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
using MeAjudaAi.Web.Admin.Tests.Helpers;

namespace MeAjudaAi.Web.Admin.Tests.Layout;

/// <summary>
/// Testes para o dark mode toggle no MainLayout.razor usando bUnit
/// </summary>
public class DarkModeToggleTests
{
    [Fact]
    public async Task MainLayout_Should_Dispatch_ToggleDarkModeAction_When_Button_Clicked()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockDispatcher = new Mock<IDispatcher>();
        var mockThemeState = new Mock<IState<ThemeState>>();

        // Configurar estado inicial (light mode)
        mockThemeState.Setup(x => x.Value).Returns(new ThemeState { IsDarkMode = false });

        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockThemeState.Object);

        var cut = ctx.Render<MainLayout>();

        // Encontrar botões que podem ser o dark mode toggle
        var buttons = cut.FindAll("button");
        buttons.Should().HaveCountGreaterThanOrEqualTo(2, "o botão de dark mode deve existir no AppBar");

        // Act - Clicar no segundo botão (toggle dark mode)
        buttons[1].Click();

        // Assert
        mockDispatcher.Verify(
            x => x.Dispatch(It.IsAny<ToggleDarkModeAction>()), 
            Times.AtLeastOnce,
            "ToggleDarkModeAction deve ser disparada ao clicar no botão");
    }

    [Fact]
    public async Task ThemeState_Should_Initialize_With_LightMode_By_Default()
    {
        // Arrange & Act
        var initialState = new ThemeState();

        // Assert
        initialState.IsDarkMode.Should().BeFalse("Estado inicial deve ser light mode");
        
        await Task.CompletedTask;
    }
}
