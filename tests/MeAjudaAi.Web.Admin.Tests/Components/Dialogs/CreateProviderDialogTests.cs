using Bunit;
using FluentAssertions;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Web.Admin.Components.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor;
using MudBlazor.Services;

namespace MeAjudaAi.Web.Admin.Tests.Components.Dialogs;

/// <summary>
/// Testes para CreateProviderDialog usando bUnit
/// </summary>
public class CreateProviderDialogTests
{
    [Fact]
    public void CreateProviderDialog_Should_Render_Form_Fields()
    {
        // Arrange
        using var ctx = new Bunit.BunitContext();
        var mockProvidersApi = new Mock<IProvidersApi>();
        var mockSnackbar = new Mock<ISnackbar>();
        
        ctx.Services.AddSingleton(mockProvidersApi.Object);
        ctx.Services.AddSingleton(mockSnackbar.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.Render<CreateProviderDialog>();

        // Assert
        var inputs = cut.FindAll("input");
        inputs.Should().NotBeEmpty("Dialog deve conter campos de input");
        
        // MudTextField uses floating labels, not placeholders - check for the component or rendered label
        var mudTextFields = cut.FindComponents<MudTextField<string>>();
        mudTextFields.Should().NotBeEmpty("Dialog deve ter campos MudTextField");
    }

    [Fact]
    public void CreateProviderDialog_Should_Have_Submit_Button()
    {
        // Arrange
        using var ctx = new Bunit.BunitContext();
        var mockProvidersApi = new Mock<IProvidersApi>();
        var mockSnackbar = new Mock<ISnackbar>();
        
        ctx.Services.AddSingleton(mockProvidersApi.Object);
        ctx.Services.AddSingleton(mockSnackbar.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.Render<CreateProviderDialog>();

        // Assert
        var buttons = cut.FindAll("button");
        buttons.Should().Contain(b => b.TextContent.Contains("Criar"), "Deve ter botão Criar");
        buttons.Should().Contain(b => b.TextContent.Contains("Cancelar"), "Deve ter botão Cancelar");
    }

    [Fact]
    public void CreateProviderDialog_Should_Show_Provider_Type_Selection()
    {
        // Arrange
        using var ctx = new Bunit.BunitContext();
        var mockProvidersApi = new Mock<IProvidersApi>();
        var mockSnackbar = new Mock<ISnackbar>();
        
        ctx.Services.AddSingleton(mockProvidersApi.Object);
        ctx.Services.AddSingleton(mockSnackbar.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.Render<CreateProviderDialog>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("Tipo de Fornecedor", "Deve ter seleção de tipo de provider");
    }

    [Fact]
    public void CreateProviderDialog_Should_Have_MudForm_Component()
    {
        // Arrange
        using var ctx = new Bunit.BunitContext();
        var mockProvidersApi = new Mock<IProvidersApi>();
        var mockSnackbar = new Mock<ISnackbar>();
        
        ctx.Services.AddSingleton(mockProvidersApi.Object);
        ctx.Services.AddSingleton(mockSnackbar.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.Render<CreateProviderDialog>();

        // Assert
        var forms = cut.FindAll("form");
        forms.Should().NotBeEmpty("Dialog deve conter formulário MudForm");
    }
}
