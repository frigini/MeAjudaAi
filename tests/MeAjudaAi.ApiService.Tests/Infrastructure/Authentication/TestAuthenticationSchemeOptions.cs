using Microsoft.AspNetCore.Authentication;

namespace MeAjudaAi.ApiService.Tests.Infrastructure.Authentication;

/// <summary>
/// Opções para o esquema de autenticação de teste
/// </summary>
public class TestAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Usuário padrão para testes
    /// </summary>
    public string DefaultUserId { get; set; } = "test-user-id";

    /// <summary>
    /// Nome do usuário padrão para testes
    /// </summary>
    public string DefaultUserName { get; set; } = "test-user";
}
