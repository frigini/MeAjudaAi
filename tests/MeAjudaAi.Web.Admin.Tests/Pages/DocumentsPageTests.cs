using Bunit;
using FluentAssertions;
using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Contracts.Modules.Documents.DTOs;
using MeAjudaAi.Web.Admin.Features.Modules.Documents;
using MeAjudaAi.Web.Admin.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using static MeAjudaAi.Web.Admin.Features.Modules.Documents.DocumentsActions;
using MeAjudaAi.Web.Admin.Tests.Helpers;

namespace MeAjudaAi.Web.Admin.Tests.Pages;

/// <summary>
/// Testes para a página Documents.razor usando bUnit
/// </summary>
public class DocumentsPageTests
{
    [Fact]
    public async Task Documents_Page_Should_Show_Provider_Selector()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockDocumentsApi = new Mock<IDocumentsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDocumentsState = new Mock<IState<DocumentsState>>();
        
        mockDocumentsState.Setup(x => x.Value).Returns(new DocumentsState());
        
        ctx.Services.AddSingleton(mockDocumentsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockDocumentsState.Object);

        // Act
        var cut = ctx.Render<Documents>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("Selecionar Provedor", "Deve ter botão de seleção de provider");
    }

    [Fact]
    public async Task Documents_Page_Should_Show_Upload_Button_When_Provider_Selected()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockDocumentsApi = new Mock<IDocumentsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDocumentsState = new Mock<IState<DocumentsState>>();
        
        var testProviderId = Guid.NewGuid();
        mockDocumentsState.Setup(x => x.Value).Returns(new DocumentsState 
        { 
            SelectedProviderId = testProviderId
        });
        
        ctx.Services.AddSingleton(mockDocumentsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockDocumentsState.Object);

        // Act
        var cut = ctx.Render<Documents>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("Upload Documento", "Deve ter botão de upload quando provider selecionado");
    }

    [Fact]
    public async Task Documents_Page_Should_Show_Loading_Indicator_When_Loading()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockDocumentsApi = new Mock<IDocumentsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDocumentsState = new Mock<IState<DocumentsState>>();
        
        mockDocumentsState.Setup(x => x.Value).Returns(new DocumentsState { IsLoading = true });
        
        ctx.Services.AddSingleton(mockDocumentsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockDocumentsState.Object);

        // Act
        var cut = ctx.Render<Documents>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("mud-progress-linear", "Indicador de loading deve estar visível quando IsLoading=true");
    }

    [Fact]
    public async Task Documents_Page_Should_Display_Document_List()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
        var mockDocumentsApi = new Mock<IDocumentsApi>();
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDocumentsState = new Mock<IState<DocumentsState>>();
        
        var testDocument = new ModuleDocumentDto
        {
            Id = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            DocumentType = "RG",
            FileName = "test.pdf",
            FileUrl = "https://example.com/test.pdf",
            Status = "Uploaded",
            UploadedAt = DateTime.UtcNow
        };
        
        mockDocumentsState.Setup(x => x.Value).Returns(new DocumentsState 
        { 
            SelectedProviderId = testDocument.ProviderId,
            Documents = new List<ModuleDocumentDto> { testDocument }
        });
        
        ctx.Services.AddSingleton(mockDocumentsApi.Object);
        ctx.Services.AddSingleton(mockDispatcher.Object);
        ctx.Services.AddSingleton(mockDocumentsState.Object);

        // Act
        var cut = ctx.Render<Documents>();

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("RG", "Deve exibir tipo de documento");
        markup.Should().Contain("test.pdf", "Deve exibir nome do arquivo");
    }

    [Fact]
    public async Task Documents_Page_Should_Show_Error_Message_When_Error_Exists()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.AddAdminTestServices();
        
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

        // Act
        var cut = ctx.Render<Documents>();

        // Assert
        var alerts = cut.FindAll(".mud-alert");
        alerts.Should().NotBeEmpty("Alert de erro deve estar visível");
    }
}
