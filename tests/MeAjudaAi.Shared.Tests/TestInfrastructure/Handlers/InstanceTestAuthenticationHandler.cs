using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;

/// <summary>
/// Handler de autenticação baseado em instância para testes que elimina problemas de estado estático.
/// Cada factory de teste obtém sua própria configuração de autenticação isolada.
/// </summary>
public class InstanceTestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ITestAuthenticationConfiguration configuration) : BaseTestAuthenticationHandler(options, logger, encoder)
{
    public const string SchemeName = "TestInstance";

    private readonly ITestAuthenticationConfiguration _configuration = configuration;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Se nenhum usuário está configurado e usuários não autenticados não são permitidos, falha a autenticação
        if (!_configuration.HasUser && !_configuration.AllowUnauthenticated)
        {
            return Task.FromResult(AuthenticateResult.Fail("No authentication configuration set"));
        }

        // Auto-configura como admin se não há configuração e usuários não autenticados são permitidos
        if (!_configuration.HasUser && _configuration.AllowUnauthenticated)
        {
            _configuration.ConfigureAdmin();
        }

        return Task.FromResult(CreateSuccessResult());
    }

    protected override string GetTestUserId() => _configuration.UserId ?? base.GetTestUserId();
    protected override string GetTestUserName() => _configuration.UserName ?? base.GetTestUserName();
    protected override string GetTestUserEmail() => _configuration.Email ?? base.GetTestUserEmail();
    protected override string[] GetTestUserRoles() => _configuration.Roles?.ToArray() ?? base.GetTestUserRoles();
    protected override string GetAuthenticationScheme() => SchemeName;
}
