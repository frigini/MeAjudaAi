using Bunit;
using FluentAssertions;
using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Contracts.Modules.Documents.DTOs;
using MeAjudaAi.Web.Admin.Features.Documents;
using MeAjudaAi.Web.Admin.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using static MeAjudaAi.Web.Admin.Features.Documents.DocumentsActions;

namespace MeAjudaAi.Web.Admin.Tests.Pages;

/// <summary>
/// Testes para a página Documents.razor usando bUnit
/// </summary>
public class DocumentsPageTests
{
    [Fact]
    public void Documents_Page_Should_Show_Provider_Selector()
    {
        // Arrange
        using var ctx = new Bunit.TestContext();
        var mockDocumentsApi = new Mock<IDocumentsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDocumentsState = new Mock<IState<DocumentsState>>();
        
        mockDocumentsState.Setup(x => x.Value).Returns(new DocumentsState());
        
        ctx.Services.AddSingleton(mockDocumentsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockDocumentsState.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.RenderComponent<Documents>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("Selecionar Fornecedor", "Deve ter botão de seleção de provider");
    }

    [Fact]
    public void Documents_Page_Should_Show_Upload_Button_When_Provider_Selected()
    {
        // Arrange
        using var ctx = new Bunit.TestContext();
        var mockDocumentsApi = new Mock<IDocumentsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDocumentsState = new Mock<IState<DocumentsState>>();
        
        var testProviderId = Guid.NewGuid();
        mockDocumentsState.Setup(x => x.Value).Returns(new DocumentsState 
        { 
            SelectedProviderId = testProviderId,
            SelectedProviderName = "Test Provider"
        });
        
        ctx.Services.AddSingleton(mockDocumentsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockDocumentsState.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.RenderComponent<Documents>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("Upload Documento", "Deve ter botão de upload quando provider selecionado");
    }

    [Fact]
    public void Documents_Page_Should_Show_Loading_Indicator_When_Loading()
    {
        // Arrange
        using var ctx = new Bunit.TestContext();
        var mockDocumentsApi = new Mock<IDocumentsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDocumentsState = new Mock<IState<DocumentsState>>();
        
        mockDocumentsState.Setup(x => x.Value).Returns(new DocumentsState { IsLoading = true });
        
        ctx.Services.AddSingleton(mockDocumentsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockDocumentsState.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.RenderComponent<Documents>();

        // Assert
        var progressBars = cut.FindAll(".mud-progress-linear");
        progressBars.Should().NotBeEmpty("Indicador de loading deve estar visível quando IsLoading=true");
    }

    [Fact]
    public void Documents_Page_Should_Display_Document_List()
    {
        // Arrange
        using var ctx = new Bunit.TestContext();
        var mockDocumentsApi = new Mock<IDocumentsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDocumentsState = new Mock<IState<DocumentsState>>();
        
        var testDocument = new ModuleDocumentDto
        {
            Id = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            DocumentType = "RG",
            FileName = "test.pdf",
            Status = "Uploaded",
            UploadedAt = DateTime.UtcNow
        };
        
        mockDocumentsState.Setup(x => x.Value).Returns(new DocumentsState 
        { 
            Documents = new List<ModuleDocumentDto> { testDocument }
        });
        
        ctx.Services.AddSingleton(mockDocumentsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockDocumentsState.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.RenderComponent<Documents>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("RG", "Deve exibir tipo de documento");
        markup.Should().Contain("test.pdf", "Deve exibir nome do arquivo");
    }

    [Fact]
    public void Documents_Page_Should_Show_Error_Message_When_Error_Exists()
    {
        // Arrange
        using var ctx = new Bunit.TestContext();
        var mockDocumentsApi = new Mock<IDocumentsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDocumentsState = new Mock<IState<DocumentsState>>();
        
        mockDocumentsState.Setup(x => x.Value).Returns(new DocumentsState 
        { 
            ErrorMessage = "Erro ao carregar documentos"
        });
        
        ctx.Services.AddSingleton(mockDocumentsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockDocumentsState.Object);
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = ctx.RenderComponent<Documents>();

        // Assert
        var alerts = cut.FindAll(".mud-alert");
        alerts.Should().NotBeEmpty("Alert de erro deve estar visível");
    }
}
