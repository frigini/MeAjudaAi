using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint responsável pela criação de novos prestadores de serviços no sistema.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para criação de prestadores utilizando
/// arquitetura CQRS. Requer autorização apropriada e valida dados antes de
/// enviar comando para processamento. Integra com o sistema de usuários.
/// </remarks>
[ExcludeFromCodeCoverage]
public class CreateProviderEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de criação de prestador.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint POST em "/" com:
    /// - Autorização por permissão (ProvidersCreate)
    /// - Documentação OpenAPI automática
    /// - Códigos de resposta apropriados
    /// - Nome único para referência
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/", CreateProviderAsync)
            .WithName("CreateProvider")
            .WithSummary("Criar novo prestador de serviços")
            .WithDescription("""
                Cria um novo prestador de serviços no sistema associado a um usuário.
                
                **Características:**
                - 👤 Vinculação obrigatória com usuário existente
                - 🏢 Suporte para prestadores individuais e empresas
                - 📋 Perfil de negócio completo com informações de contato
                - 📍 Endereço principal obrigatório
                - ✅ Criação com status de verificação pendente
                
                **Campos obrigatórios:**
                - UserId: ID de usuário válido e existente
                - Name: Nome do prestador
                - Type: Individual ou Company
                - BusinessProfile: Informações completas do negócio
                """)
            .Produces<Response<ProviderDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequirePermission(EPermission.ProvidersCreate);

    /// <summary>
    /// Processa requisição de criação de prestador de forma assíncrona.
    /// </summary>
    /// <param name="request">Dados do prestador a ser criado</param>
    /// <param name="commandDispatcher">Dispatcher para envio de comandos CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado HTTP contendo:
    /// - 201 Created: Prestador criado com sucesso e dados do prestador
    /// - 400 Bad Request: Erro de validação ou criação
    /// </returns>
    /// <remarks>
    /// Fluxo de execução:
    /// 1. Converte request em comando CQRS
    /// 2. Envia comando através do dispatcher
    /// 3. Processa resultado e retorna resposta HTTP apropriada
    /// 4. Inclui localização do recurso criado no header
    /// </remarks>
    private static async Task<IResult> CreateProviderAsync(
        [FromBody] CreateProviderRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return Results.BadRequest("Corpo da requisição é obrigatório");

        var command = request.ToCommand();
        var result = await commandDispatcher.SendAsync<CreateProviderCommand, Result<ProviderDto>>(
            command, cancellationToken);

        // Protege contra falha ou valor nulo para evitar CS8602 ao acessar result.Value.Id
        if (!result.IsSuccess || result.Value is null)
            return Handle(result);

        var provider = result.Value;
        return Handle(result, "GetProviderById", new { id = provider.Id });
    }
}
