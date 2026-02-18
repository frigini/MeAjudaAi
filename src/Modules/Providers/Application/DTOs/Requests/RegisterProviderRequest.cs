using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.Application.DTOs.Requests;

/// <summary>
/// Request para auto-registro de um novo prestador de serviços na plataforma.
/// Endpoint público — não requer autenticação.
/// </summary>
/// <remarks>
/// Este é o passo inicial do onboarding. Após o registro, o prestador receberá
/// o role 'provider-standard' no Keycloak e será redirecionado para o wizard
/// de completar o perfil (PUT /api/v1/providers/me/profile).
/// </remarks>
public record RegisterProviderRequest
{
    /// <summary>
    /// Nome completo ou nome fantasia do prestador.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Tipo do prestador de serviços.
    /// </summary>
    public EProviderType Type { get; init; }

    /// <summary>
    /// Número de telefone profissional (obrigatório).
    /// </summary>
    public string PhoneNumber { get; init; } = string.Empty;

    /// <summary>
    /// Email profissional do prestador.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Indica que o prestador aceitou os Termos de Uso (obrigatório).
    /// </summary>
    public bool AcceptedTerms { get; init; }

    /// <summary>
    /// Indica que o prestador aceitou a Política de Privacidade/LGPD (obrigatório).
    /// </summary>
    public bool AcceptedPrivacyPolicy { get; init; }
}
