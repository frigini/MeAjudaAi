using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Payments.Application.Options;
using MeAjudaAi.Shared.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MeAjudaAi.Modules.Payments.Application.Services;

public class ReturnUrlResolver(
    IConfiguration configuration,
    PaymentsOptions paymentsOptions,
    IHostEnvironment environment,
    ILogger<ReturnUrlResolver> logger) : IReturnUrlResolver
{
    public Result<string> Resolve(string? returnUrl, Guid providerId)
    {
        var clientBaseUrl = configuration["ClientBaseUrl"];
        if (string.IsNullOrEmpty(clientBaseUrl))
        {
            return Result<string>.Failure(Error.Internal("ClientBaseUrl não configurada."));
        }

        clientBaseUrl = clientBaseUrl.TrimEnd('/');
        var normalized = (returnUrl ?? "").Trim();

        if (normalized.Equals("account", StringComparison.OrdinalIgnoreCase))
        {
            return Result<string>.Success($"{clientBaseUrl}/account");
        }

        if (normalized.Equals("billing", StringComparison.OrdinalIgnoreCase))
        {
            return Result<string>.Success($"{clientBaseUrl}/billing");
        }

        if (string.IsNullOrWhiteSpace(normalized))
        {
            logger.LogInformation(
                "Billing portal ReturnUrl empty, falling back to ClientBaseUrl for Provider {ProviderId}.",
                providerId);
            return Result<string>.Success(clientBaseUrl);
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
        {
            logger.LogInformation(
                "Billing portal ReturnUrl invalid, falling back to ClientBaseUrl for Provider {ProviderId}.",
                providerId);
            return Result<string>.Success(clientBaseUrl);
        }

        if (!IsTrustedHost(uri))
        {
            logger.LogWarning(
                "Blocked billing portal redirect to untrusted host {Host} for Provider {ProviderId}.",
                uri.Host, providerId);
            return Result<string>.Failure(Error.BadRequest($"ReturnUrl host '{uri.Host}' não é confiável."));
        }

        return Result<string>.Success(normalized);
    }

    private bool IsTrustedHost(Uri uri)
    {
        if (uri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        var trustedHosts = new HashSet<string>(paymentsOptions.AllowedReturnHosts, StringComparer.OrdinalIgnoreCase);

        var clientBaseUrl = configuration["ClientBaseUrl"];
        if (!string.IsNullOrEmpty(clientBaseUrl) && Uri.TryCreate(clientBaseUrl, UriKind.Absolute, out var clientUri))
        {
            trustedHosts.Add(clientUri.Host);
        }

        if (EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
        {
            trustedHosts.Add("localhost");
            trustedHosts.Add("127.0.0.1");
            trustedHosts.Add("::1");
        }

        return trustedHosts.Contains(uri.Host);
    }
}
