using Asp.Versioning;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.Documents.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public DocumentsController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    /// <summary>
    /// Gera URL de upload com SAS token para envio direto ao blob storage
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(UploadDocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateUploadUrl(
        [FromBody] UploadDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UploadDocumentCommand(
            request.ProviderId,
            request.DocumentType.ToString(),
            request.FileName,
            request.ContentType,
            request.FileSizeBytes);

        var response = await _commandDispatcher.SendAsync<UploadDocumentCommand, UploadDocumentResponse>(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Obtém o status de um documento específico
    /// </summary>
    [HttpGet("{documentId:guid}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentStatus(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var query = new GetDocumentStatusQuery(documentId);
        var result = await _queryDispatcher.QueryAsync<GetDocumentStatusQuery, DocumentDto?>(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Lista todos os documentos de um provedor
    /// </summary>
    [HttpGet("provider/{providerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviderDocuments(
        Guid providerId,
        CancellationToken cancellationToken)
    {
        var query = new GetProviderDocumentsQuery(providerId);
        var result = await _queryDispatcher.QueryAsync<GetProviderDocumentsQuery, IEnumerable<DocumentDto>>(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Solicita verificação manual de um documento (se OCR falhar)
    /// </summary>
    [HttpPost("{documentId:guid}/verify")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestVerification(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var command = new RequestVerificationCommand(documentId);
        await _commandDispatcher.SendAsync(command, cancellationToken);
        return Accepted();
    }
}
